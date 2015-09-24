using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.Tasking {
  public class Tasks {
    public static readonly Tasks Empty = new Tasks(ImmutableDictionary<string, ITaskDefinition>.Empty);
    private ImmutableDictionary<string, ITaskDefinition> TaskDefinitions { get; }

    private Tasks(ImmutableDictionary<string, ITaskDefinition> taskDefinitions) {
      TaskDefinitions = taskDefinitions;
    }

    public Tasks SetAsync<T>(string taskName, Func<ITasker, Task<T>> task) {
      ITaskDefinition previousTaskDefinition;
      if (TaskDefinitions.TryGetValue(taskName, out previousTaskDefinition)) {
        if (previousTaskDefinition.ReturnType != typeof(T)) {
          throw new TaskTypeOverrideException($"Could not redefine the type of task '{taskName}' from '{previousTaskDefinition.ReturnType}' to '{typeof(T)}'. Redefinition of task types is not allowed.");
        }
      }
      return new Tasks(TaskDefinitions.SetItem(taskName, new TaskDefinition<T>(task)));
    }

    public Tasks ModifyAsync<T>(string taskName, Func<ITasker, Task<T>, Task<T>> task) {
      ITaskDefinition previousTaskDefinition;
      if (TaskDefinitions.TryGetValue(taskName, out previousTaskDefinition)) {
        if (previousTaskDefinition.ReturnType != typeof(T)) {
          throw new TaskTypeOverrideException($"Could not redefine the type of task '{taskName}' from '{previousTaskDefinition.ReturnType}' to '{typeof(T)}'. Redefinition of task types is not allowed.");
        }
        return new Tasks(TaskDefinitions.SetItem(taskName, new TaskDefinition<T>(((TaskDefinition<T>) previousTaskDefinition).Task, task)));
      }
      throw new TaskUndefinedException($"Could not modify the task '{taskName}'. The task is not defined yet.");
    }

    public bool IsTaskDefined(string taskName) => TaskDefinitions.ContainsKey(taskName);

    public bool TryGetTask<T>(string taskName, out Func<ITasker, Task<T>> task) {
      ITaskDefinition taskDefinition;
      if (TaskDefinitions.TryGetValue(taskName, out taskDefinition)) {
        AssertTaskTypedCorrectly<T>(taskName, taskDefinition.ReturnType);
        task = ((TaskDefinition<T>) taskDefinition).Task;
        return true;
      }
      task = null;
      return false;
    }

    public Task<T> InvokeTask<T>(string taskName, ITasker tasker) {
      Func<ITasker, Task<T>> taskDefinition;
      if (TryGetTask(taskName, out taskDefinition)) {
        return taskDefinition(tasker);
      }
      throw new TaskUndefinedException($"Task '{taskName ?? "<null>"}' is undefined.");
    }

    private interface ITaskDefinition {
      Type ReturnType { get; }
    }

    private class TaskDefinition<T> : ITaskDefinition {
      public Type ReturnType => typeof(T);
      public Func<ITasker, Task<T>> Task { get; }

      public TaskDefinition(Func<ITasker, Task<T>> originalTask, Func<ITasker, Task<T>, Task<T>> modifierTask) {
        Task = context => modifierTask(context, originalTask(context));
      }

      public TaskDefinition(Func<ITasker, Task<T>> originalTask) {
        Task = originalTask;
      }
    }

    internal static void AssertTaskTypedCorrectly<T>(string taskName, Type actualTaskReturnType) {
      if (actualTaskReturnType != typeof(T)) {
        throw new TaskReturnsDifferentTypeException($"Task '{taskName}' returns '{actualTaskReturnType.FullName}' but was expected to return '{typeof(T).FullName}'.");
      }
    }
  }
}