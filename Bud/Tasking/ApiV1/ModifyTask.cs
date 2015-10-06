using System;
using System.Threading.Tasks;

namespace Bud.Tasking.ApiV1 {
  public class ModifyTask<T> : ITaskModification {
    public string Name { get; }
    private Func<ITasks, Task<T>, Task<T>> Task { get; }

    public ModifyTask(Key<T> name, Func<ITasks, Task<T>, Task<T>> task) {
      Name = name;
      Task = task;
    }

    public TaskDefinition Modify(TaskDefinition taskDefinition) {
      return new TaskDefinition(typeof(T), tasks => Task(tasks, (Task<T>) taskDefinition.Task(tasks)));
    }

    public TaskDefinition ToTaskDefinition() {
      throw new TaskDefinitionException(Name, typeof(T), "Could not modify the task. No previous definition exists.");
    } 
  }
}