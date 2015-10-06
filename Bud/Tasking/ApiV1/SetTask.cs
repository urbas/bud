using System;
using System.Threading.Tasks;

namespace Bud.Tasking.ApiV1 {
  public class SetTask<T> : ITaskModification {
    public SetTask(Key<T> name, Func<ITasks, Task<T>> task) {
      Name = name;
      Task = task;
    }

    public Func<ITasks, Task<T>> Task { get; }
    public string Name { get; }
    public TaskDefinition Modify(TaskDefinition taskDefinition) => ToTaskDefinition();
    public TaskDefinition ToTaskDefinition() => new TaskDefinition(typeof(T), Task);
  }
}