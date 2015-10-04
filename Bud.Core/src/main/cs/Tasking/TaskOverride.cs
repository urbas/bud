using System;
using System.Threading.Tasks;

namespace Bud.Tasking {
  internal class TaskOverride<T> : ITaskModification {
    public string Name { get; }
    private Func<ITasks, Task<T>> Task { get; }

    public TaskOverride(string name, Func<ITasks, Task<T>> originalTask) {
      Task = originalTask;
      Name = name;
    }

    public TaskDefinition Modify(TaskDefinition taskDefinition) {
      Tasks.AssertTaskTypeIsSame<T>(Name, taskDefinition);
      return ToTaskDefinition();
    }

    public TaskDefinition ToTaskDefinition() => new TaskDefinition(typeof(T), Task);
  }
}