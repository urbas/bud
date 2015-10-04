using System;

namespace Bud.Tasking {
  internal class TaskNestingModification : ITaskModification {
    public string Name { get; }
    private string Prefix { get; }
    private ITaskModification NestedModification { get; }

    public TaskNestingModification(string prefix, ITaskModification nestedModification) {
      Prefix = prefix;
      NestedModification = nestedModification;
      Name = prefix + "/" + nestedModification.Name;
    }

    public TaskDefinition Modify(TaskDefinition taskDefinition) {
      var modifiedTaskDefinition = NestedModification.Modify(taskDefinition);
      return new TaskDefinition(modifiedTaskDefinition.ReturnType, tasks => {
        var prefixedTasks = new NestedTasks(Prefix, tasks);
        return modifiedTaskDefinition.Task(prefixedTasks);
      });
    }

    public TaskDefinition ToTaskDefinition() {
      try {
        var taskDefinition = NestedModification.ToTaskDefinition();
        return new TaskDefinition(taskDefinition.ReturnType, tasks => taskDefinition.Task(new NestedTasks(Prefix, tasks)));
      } catch (Exception e) {
        throw new TaskUndefinedException($"Could not define the task '{Name}'.", e);
      }
    }
  }
}