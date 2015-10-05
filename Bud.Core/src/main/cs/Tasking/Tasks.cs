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

    public Tasks Set<T>(Key<T> taskName, Func<ITasks, Task<T>, Task<T>> task) => Add(new TaskModification<T>(taskName, task));
    public Tasks ExtendWith(Tasks tasks) => new Tasks(TaskModifications.Concat(tasks.TaskModifications));
    public Tasks Nest(string prefix) => new Tasks(TaskModifications.Select(taskModification => new TaskNesting(prefix, taskModification)));

    internal IDictionary<string, TaskDefinition> Compile() {
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
        TasksExtensions.AssertTaskTypeIsSame<T>(Name, taskDefinition);
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
  }
}