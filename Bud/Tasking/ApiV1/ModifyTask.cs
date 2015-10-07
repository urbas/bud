using System;

namespace Bud.Tasking.ApiV1 {
  public class ModifyTask<T> : ITaskModification {
    public string Name { get; }
    private Func<ITasks, T, T> Task { get; }

    public ModifyTask(Key<T> name, Func<ITasks, T, T> task) {
      Name = name;
      Task = task;
    }

    public ITaskDefinition Modify(ITaskDefinition taskDefinition) {
      return new TaskDefinition<T>(tasks => Task(tasks, (T) taskDefinition.Invoke(tasks)));
    }

    public ITaskDefinition ToTaskDefinition() {
      throw new TaskDefinitionException(Name, typeof(T), "Could not modify the task. No previous definition exists.");
    }
  }
}