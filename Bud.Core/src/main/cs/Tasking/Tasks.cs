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

    public Tasks SetAsync<T>(string taskName, Func<IContext, Task<T>> task) {
      ITaskDefinition previousTaskDefinition;
      if (TaskDefinitions.TryGetValue(taskName, out previousTaskDefinition)) {
        if (previousTaskDefinition.ReturnType != typeof(T)) {
          throw new TaskTypeOverrideException($"Could not redefine the type of task '{taskName}' from '{previousTaskDefinition.ReturnType}' to '{typeof(T)}'. Redefinition of task types is not allowed.");
        }
      }
      return new Tasks(TaskDefinitions.SetItem(taskName, new TaskDefinition<T>(task)));
    }

    public Tasks ModifyAsync<T>(string taskName, Func<IContext, Task<T>, Task<T>> task) {
      ITaskDefinition previousTaskDefinition;
      if (TaskDefinitions.TryGetValue(taskName, out previousTaskDefinition)) {
        if (previousTaskDefinition.ReturnType != typeof(T)) {
          throw new TaskTypeOverrideException($"Could not redefine the type of task '{taskName}' from '{previousTaskDefinition.ReturnType}' to '{typeof(T)}'. Redefinition of task types is not allowed.");
        }
        return new Tasks(TaskDefinitions.SetItem(taskName, new TaskDefinition<T>(((TaskDefinition<T>) previousTaskDefinition).Task, task)));
      }
      throw new TaskUndefinedException($"Could not modify the task '{taskName}'. The task is not defined yet.");
    }

    public Task<T> InvokeTask<T>(string taskName, IContext context) {
      ITaskDefinition taskDefinition;
      if (TaskDefinitions.TryGetValue(taskName, out taskDefinition)) {
        Context.AssertTaskTypedCorrectly<T>(taskName, taskDefinition.ReturnType);
        var typedTaskDefinition = (TaskDefinition<T>) taskDefinition;
        return typedTaskDefinition.Task(context);
      }
      throw new TaskUndefinedException($"Task '{taskName ?? "<null>"}' is undefined.");
    }

    private interface ITaskDefinition {
      Type ReturnType { get; }
    }

    private class TaskDefinition<T> : ITaskDefinition {
      public Type ReturnType => typeof(T);
      public Func<IContext, Task<T>> Task { get; }

      public TaskDefinition(Func<IContext, Task<T>> originalTask, Func<IContext, Task<T>, Task<T>> modifierTask) {
        Task = context => modifierTask(context, originalTask(context));
      }

      public TaskDefinition(Func<IContext, Task<T>> originalTask) {
        Task = originalTask;
      }
    }
  }
}