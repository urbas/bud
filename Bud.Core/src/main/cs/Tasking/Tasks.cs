using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.Tasking {
  public class Tasks : ITasks {
    public readonly static Tasks New = new Tasks();
    private ImmutableList<ITaskModification> TaskModifications { get; }

    private Tasks() : this(ImmutableList<ITaskModification>.Empty) {}

    private Tasks(ImmutableList<ITaskModification> taskModifications) {
      TaskModifications = taskModifications;
    }

    public Tasks SetAsync<T>(string taskName, Func<ITasks, Task<T>> task) => Add(new TaskOverride<T>(taskName, task));

    public Tasks ModifyAsync<T>(string taskName, Func<ITasks, Task<T>, Task<T>> task) => Add(new TaskModification<T>(taskName, task));

    private Tasks Add(ITaskModification taskModification) => new Tasks(TaskModifications.Add(taskModification));

    public Tasks ExtendedWith(Tasks tasks) => new Tasks(TaskModifications.AddRange(tasks.TaskModifications));

    public IDictionary<string, TaskDefinition> Compile() {
      var taskDefinitions = new Dictionary<string, TaskDefinition>();
      foreach (var taskModification in TaskModifications) {
        TaskDefinition existingTaskDefinition;
        if (taskDefinitions.TryGetValue(taskModification.Name, out existingTaskDefinition)) {
          taskDefinitions[taskModification.Name] = taskModification.Extend(existingTaskDefinition);
        } else {
          taskDefinitions.Add(taskModification.Name, taskModification.ToTaskDefinition());
        }
      }
      return taskDefinitions;
    }

    public Task<T> Invoke<T>(string taskName) => new TasksResultCache(Compile()).Invoke<T>(taskName);

    public Task Invoke(string taskName) => new TasksResultCache(Compile()).Invoke(taskName);

    private interface ITaskModification {
      string Name { get; }
      TaskDefinition Extend(TaskDefinition taskDefinition);
      TaskDefinition ToTaskDefinition();
    }

    private class TaskModification<T> : ITaskModification {
      public string Name { get; }
      private Func<ITasks, Task<T>, Task<T>> TaskModifier { get; }

      public TaskModification(string name, Func<ITasks, Task<T>, Task<T>> taskModifier) {
        Name = name;
        TaskModifier = taskModifier;
      }

      public TaskDefinition Extend(TaskDefinition taskDefinition) {
        AssertTaskTypeIsSame<T>(Name, taskDefinition);
        return new TaskDefinition(typeof(T), tasker => TaskModifier(tasker, (Task<T>) taskDefinition.Task(tasker)));
      }

      public TaskDefinition ToTaskDefinition() {
        throw new TaskUndefinedException($"Could not modify the task '{Name}'. The task is not defined yet.");
      }
    }

    private static void AssertTaskTypeIsSame<T>(string taskName, TaskDefinition previousTaskDefinition) {
      if (previousTaskDefinition.ReturnType != typeof(T)) {
        throw new TaskTypeOverrideException($"Could not redefine the type of task '{taskName}' from '{previousTaskDefinition.ReturnType}' to '{typeof(T)}'. Redefinition of task types is not allowed.");
      }
    }

    private class TaskOverride<T> : ITaskModification {
      public string Name { get; }
      private Func<ITasks, Task<T>> Task { get; }

      public TaskOverride(string name, Func<ITasks, Task<T>> originalTask) {
        Task = originalTask;
        Name = name;
      }

      public TaskDefinition Extend(TaskDefinition taskDefinition) {
        AssertTaskTypeIsSame<T>(Name, taskDefinition);
        return ToTaskDefinition();
      }

      public TaskDefinition ToTaskDefinition() => new TaskDefinition(typeof(T), Task);
    }
  }
}