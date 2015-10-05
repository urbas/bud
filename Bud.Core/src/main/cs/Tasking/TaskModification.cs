using System;
using System.Threading.Tasks;

namespace Bud.Tasking {
  public class TaskModification<T> : ITaskModification {
    public string Name { get; }
    private Func<ITasks, Task<T>, Task<T>> Task { get; }

    public TaskModification(Key<T> name, Func<ITasks, Task<T>, Task<T>> task) {
      Name = name;
      Task = task;
    }

    public TaskDefinition Modify(TaskDefinition taskDefinition) {
      TasksExtensions.AssertTaskTypeIsSame<T>(Name, taskDefinition);
      return new TaskDefinition(typeof(T), tasks => Task(tasks, (Task<T>) taskDefinition.Task(tasks)));
    }

    public TaskDefinition ToTaskDefinition() => new TaskDefinition(typeof(T), tasks => Task(tasks, null));
  }
}