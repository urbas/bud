using System;
using System.Threading.Tasks;

namespace Bud.Tasking {
  internal class TaskDefinition<T> : ITaskDefinition {
    public string TaskName { get; }
    public Type ReturnType => typeof(T);
    public Func<IContext, Task<T>> Task { get; }

    public TaskDefinition(string taskName, Func<IContext, Task<T>> originalTask, Func<IContext, Task<T>, Task<T>> modifierTask) {
      TaskName = taskName;
      Task = context => modifierTask(context, originalTask(context));
    }

    public TaskDefinition(string taskName, Func<IContext, Task<T>> originalTask) {
      TaskName = taskName;
      Task = originalTask;
    }
  }
}