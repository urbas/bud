using System;
using System.Threading.Tasks;

namespace Bud.Tasking.ApiV1 {
  public class InitializeTask<T> : ITaskModification {
    public InitializeTask(Key<T> name, Func<ITasks, Task<T>> task) {
      Name = name;
      Task = task;
    }

    public string Name { get; }
    public Func<ITasks, Task<T>> Task { get; }
    public TaskDefinition Modify(TaskDefinition taskDefinition) => taskDefinition;
    public TaskDefinition ToTaskDefinition() => new TaskDefinition(typeof(T), Task);
  }
}