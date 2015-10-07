namespace Bud.Tasking.ApiV1 {
  public class TaskNesting : ITaskModification {
    private string Prefix { get; }
    private ITaskModification NestedModification { get; }

    public TaskNesting(string prefix, ITaskModification nestedModification) {
      Prefix = prefix;
      NestedModification = nestedModification;
      Name = prefix + "/" + nestedModification.Name;
    }

    public string Name { get; }

    public ITaskDefinition Modify(ITaskDefinition taskDefinition) {
      var modifiedTaskDefinition = NestedModification.Modify(taskDefinition);
      return new TaskDefinition(modifiedTaskDefinition.ReturnType, tasks => {
        var prefixedTasks = new NestedTasks(Prefix, tasks);
        return modifiedTaskDefinition.Invoke(prefixedTasks);
      });
    }

    public ITaskDefinition ToTaskDefinition() {
      var taskDefinition = NestedModification.ToTaskDefinition();
      return new TaskDefinition(taskDefinition.ReturnType, tasks => taskDefinition.Invoke(new NestedTasks(Prefix, tasks)));
    }
  }
}