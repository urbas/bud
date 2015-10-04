using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Bud.Tasking {
  public class Tasks : ITasks {
    public static readonly Tasks New = new Tasks();
    private ImmutableList<ITaskModification> TaskModifications { get; }

    private Tasks() : this(ImmutableList<ITaskModification>.Empty) {}

    private Tasks(ImmutableList<ITaskModification> taskModifications) {
      TaskModifications = taskModifications;
    }

    public Task Get(Key taskName) => new TasksResultCache(Compile()).Get(taskName);
    public Task<T> Get<T>(Key<T> taskName) => new TasksResultCache(Compile()).Get<T>(taskName);
    public Tasks Const<T>(Key<T> taskName, T value) => Add(new TaskOverride<T>(taskName, tasks => Task.FromResult(value)));
    public Tasks Set<T>(Key<T> taskName, Func<T> task) => Add(new TaskOverride<T>(taskName, tasks => Task.FromResult(task())));
    public Tasks SetAsync<T>(Key<T> taskName, Func<ITasks, Task<T>> task) => Add(new TaskOverride<T>(taskName, task));
    public Tasks Modify<T>(Key<T> taskName, Func<T, T> task) => Add(new TaskModification<T>(taskName, async (tasks, oldTask) => task(await oldTask)));
    public Tasks ModifyAsync<T>(Key<T> taskName, Func<ITasks, Task<T>, Task<T>> task) => Add(new TaskModification<T>(taskName, task));
    public Tasks ExtendWith(Tasks tasks) => new Tasks(TaskModifications.AddRange(tasks.TaskModifications));
    public Tasks Nest(string prefix, Tasks tasks) => new Tasks(tasks.TaskModifications.Select(taskModification => new TaskNestingModification(prefix, taskModification) as ITaskModification).ToImmutableList());

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

    private Tasks Add(ITaskModification taskModification) => new Tasks(TaskModifications.Add(taskModification));

    internal static void AssertTaskTypeIsSame<T>(string taskName, TaskDefinition previousTaskDefinition) {
      if (previousTaskDefinition.ReturnType != typeof(T)) {
        throw new TaskReturnTypeException($"Could not redefine the type of task '{taskName}' from '{previousTaskDefinition.ReturnType}' to '{typeof(T)}'. Redefinition of task types is not allowed.");
      }
    }
  }
}