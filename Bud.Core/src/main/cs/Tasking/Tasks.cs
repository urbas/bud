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
    public Tasks Nest(string prefix) => new Tasks(TaskModifications.Select(taskModification => new TaskNestingModification(prefix, taskModification)));

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

    internal Tasks Add(ITaskModification taskModification) => new Tasks(TaskModifications.Concat(new [] {taskModification}));
  }
}