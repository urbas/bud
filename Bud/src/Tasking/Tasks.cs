using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Bud.Tasking {
  public class Tasks : ITasks {
    public static readonly Tasks NewTasks = new Tasks();
    private IEnumerable<ITaskModification> TaskModifications { get; }

    private Tasks() : this(ImmutableArray<ITaskModification>.Empty) {}

    private Tasks(IEnumerable<ITaskModification> taskModifications) {
      TaskModifications = taskModifications;
    }

    /// <summary>
    ///   Asynchronously invokes the task with the given name and returns the result of the task.
    /// </summary>
    /// <param name="taskName">the name of the task to invoke.</param>
    public Task Get(Key taskName) => new TasksResultCache(Compile()).Get(taskName);

    /// <summary>
    ///   Asynchronously invokes the task with the given name and returns the typed result of the task.
    /// </summary>
    /// <param name="taskName">the name of the task to invoke.</param>
    /// <exception cref="TaskReturnTypeException">
    ///   thrown if the actual type of the task does not
    ///   match the requested type of the task.
    /// </exception>
    /// <typeparam name="T">the requested type of the task's result.</typeparam>
    public Task<T> Get<T>(Key<T> taskName) => new TasksResultCache(Compile()).Get(taskName);

    /// <summary>
    ///   Initialises or modifies a task.
    /// </summary>
    /// <typeparam name="T">the return type of the task.</typeparam>
    /// <returns>a copy of these tasks containing the new definition of the task.</returns>
    /// <remarks>
    ///   <para>
    ///     If the task with the given name is already defined, then the second parameter to the <paramref name="task" />
    ///     function will be non-<c>null</c> and can be used to modify the previous value of the task.
    ///   </para>
    ///   <para>
    ///     If the task has not been defined yet, then the second parameter to the <paramref name="task" />
    ///     function will be <c>null</c>.
    ///   </para>
    /// </remarks>
    public Tasks Set<T>(Key<T> taskName, Func<ITasks, Task<T>, Task<T>> task) => Add(new TaskModification<T>(taskName, task));

    /// <returns>a copy of these tasks with added task definitions from <paramref name="tasks" />.</returns>
    public Tasks ExtendWith(Tasks tasks) => new Tasks(TaskModifications.Concat(tasks.TaskModifications));

    /// <returns>a copy of these tasks where the name of every task is prefixed with the given string and a slash character.</returns>
    public Tasks Nest(string prefix) => new Tasks(TaskModifications.Select(taskModification => new TaskNesting(prefix, taskModification)));

    private IDictionary<string, TaskDefinition> Compile() {
      var taskDefinitions = new Dictionary<string, TaskDefinition>();
      foreach (var taskModification in TaskModifications) {
        TaskDefinition existingTaskDefinition;
        if (taskDefinitions.TryGetValue(taskModification.Name, out existingTaskDefinition)) {
          taskDefinitions[taskModification.Name] = taskModification.Modify(existingTaskDefinition);
        } else {
          taskDefinitions.Add(taskModification.Name, taskModification.ToTaskDefinition());
        }
      }
      return taskDefinitions;
    }

    private Tasks Add(ITaskModification taskModification) => new Tasks(TaskModifications.Concat(new[] {taskModification}));

    private interface ITaskModification {
      string Name { get; }
      TaskDefinition Modify(TaskDefinition taskDefinition);
      TaskDefinition ToTaskDefinition();
    }

    private class TaskModification<T> : ITaskModification {
      public string Name { get; }
      private Func<ITasks, Task<T>, Task<T>> Task { get; }

      public TaskModification(Key<T> name, Func<ITasks, Task<T>, Task<T>> task) {
        Name = name;
        Task = task;
      }

      public TaskDefinition Modify(TaskDefinition taskDefinition) {
        TasksExtensions.AssertTaskTypeIsSame<T>(Name, taskDefinition.ReturnType);
        return new TaskDefinition(typeof(T), tasks => Task(tasks, (Task<T>) taskDefinition.Task(tasks)));
      }

      public TaskDefinition ToTaskDefinition() => new TaskDefinition(typeof(T), tasks => Task(tasks, null));
    }

    private class TaskNesting : ITaskModification {
      public string Name { get; }
      private string Prefix { get; }
      private ITaskModification NestedModification { get; }

      public TaskNesting(string prefix, ITaskModification nestedModification) {
        Prefix = prefix;
        NestedModification = nestedModification;
        Name = prefix + "/" + nestedModification.Name;
      }

      public TaskDefinition Modify(TaskDefinition taskDefinition) {
        var modifiedTaskDefinition = NestedModification.Modify(taskDefinition);
        return new TaskDefinition(modifiedTaskDefinition.ReturnType, tasks => {
          var prefixedTasks = new NestedTasks(Prefix, tasks);
          return modifiedTaskDefinition.Task(prefixedTasks);
        });
      }

      public TaskDefinition ToTaskDefinition() {
        var taskDefinition = NestedModification.ToTaskDefinition();
        return new TaskDefinition(taskDefinition.ReturnType, tasks => taskDefinition.Task(new NestedTasks(Prefix, tasks)));
      }
    }

    private class NestedTasks : ITasks {
      private readonly string prefix;
      private readonly ITasks tasks;

      public NestedTasks(string prefix, ITasks tasks) {
        this.prefix = prefix + "/";
        this.tasks = tasks;
      }

      public Task<T> Get<T>(Key<T> taskName) => tasks.Get<T>(prefix + taskName);
      public Task Get(Key taskName) => tasks.Get(prefix + taskName);
    }

    private class TasksResultCache : ITasks {
      private IDictionary<string, TaskDefinition> Tasks { get; }
      private ImmutableDictionary<string, TaskResult> taskResultCache = ImmutableDictionary<string, TaskResult>.Empty;
      private readonly object taskResultCacheGuard = new object();

      internal TasksResultCache(IDictionary<string, TaskDefinition> tasks) {
        Tasks = tasks;
      }

      public Task<T> Get<T>(Key<T> taskName) {
        var taskResult = GetFromCacheOrInvoke(taskName);
        TasksExtensions.AssertTaskTypeIsSame<T>(taskName, taskResult.ResultType);
        return (Task<T>) taskResult.Result;
      }

      public Task Get(Key taskName) => GetFromCacheOrInvoke(taskName).Result;

      private TaskResult GetFromCacheOrInvoke(string taskName) {
        TaskResult taskResult;
        return taskResultCache.TryGetValue(taskName, out taskResult) ? taskResult : InvokeTaskAndCache(taskName);
      }

      private TaskResult InvokeTaskAndCache(string taskName) {
        lock (taskResultCacheGuard) {
          TaskResult taskResult;
          if (taskResultCache.TryGetValue(taskName, out taskResult)) {
            return taskResult;
          }
          TaskDefinition taskDefinition;
          if (Tasks.TryGetValue(taskName, out taskDefinition)) {
            taskResult = new TaskResult {ResultType = taskDefinition.ReturnType, Result = taskDefinition.Task(this)};
            taskResultCache = taskResultCache.Add(taskName, taskResult);
            return taskResult;
          }
          throw new TaskUndefinedException($"Task '{taskName ?? "<null>"}' is undefined.");
        }
      }

      private struct TaskResult {
        public Type ResultType;
        public Task Result;
      }
    }

    private class TaskDefinition {
      public Type ReturnType { get; }
      public Func<ITasks, Task> Task { get; }

      public TaskDefinition(Type returnType, Func<ITasks, Task> task) {
        ReturnType = returnType;
        Task = task;
      }
    }
  }
}