using System;

namespace Bud.Tasking.ApiV1 {
  public class TaskDefinition : ITaskDefinition {
    public Type ReturnType { get; }
    public Func<ITasks, object> Task { get; }

    public TaskDefinition(Type returnType, Func<ITasks, object> task) {
      ReturnType = returnType;
      Task = task;
    }

    public object Invoke(ITasks tasks) => Task(tasks);
  }

  public class TaskDefinition<T> : ITaskDefinition {
    public Func<ITasks, T> Task { get; }
    public Type ReturnType => typeof(T);
    public object Invoke(ITasks tasks) => Task(tasks);

    public TaskDefinition(Func<ITasks, T> task) {
      Task = task;
    }
  }
}