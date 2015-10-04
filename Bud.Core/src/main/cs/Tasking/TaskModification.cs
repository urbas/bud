using System;
using System.Threading.Tasks;

namespace Bud.Tasking {
  internal class TaskModification<T> : ITaskModification {
    public string Name { get; }
    private Func<ITasks, Task<T>, Task<T>> TaskModifier { get; }

    public TaskModification(string name, Func<ITasks, Task<T>, Task<T>> taskModifier) {
      Name = name;
      TaskModifier = taskModifier;
    }

    public TaskDefinition Modify(TaskDefinition taskDefinition) {
      Tasks.AssertTaskTypeIsSame<T>(Name, taskDefinition);
      return new TaskDefinition(typeof(T), tasker => TaskModifier(tasker, (Task<T>) taskDefinition.Task(tasker)));
    }

    public TaskDefinition ToTaskDefinition() {
      throw new TaskUndefinedException($"Could not modify the task '{Name}'. The task is not defined yet.");
    }
  }
}