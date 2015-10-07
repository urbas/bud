using System;

namespace Bud.Tasking.ApiV1 {
  public class SetTask<T> : ITaskModification {
    public SetTask(Key<T> name, Func<ITasks, T> task) {
      Name = name;
      Task = task;
    }

    public Func<ITasks, T> Task { get; }
    public string Name { get; }
    public ITaskDefinition Modify(ITaskDefinition taskDefinition) => ToTaskDefinition();
    public ITaskDefinition ToTaskDefinition() => new TaskDefinition<T>(Task);
  }
}