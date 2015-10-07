using System;

namespace Bud.Tasking.ApiV1 {
  public class InitializeTask<T> : ITaskModification {
    public InitializeTask(Key<T> name, Func<ITasks, T> task) {
      Name = name;
      Task = task;
    }

    public string Name { get; }
    public Func<ITasks, T> Task { get; }
    public ITaskDefinition Modify(ITaskDefinition taskDefinition) => taskDefinition;
    public ITaskDefinition ToTaskDefinition() => new TaskDefinition<T>(Task);
  }
}