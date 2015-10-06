namespace Bud.Tasking.ApiV1 {
  public interface ITaskModification {
    string Name { get; }
    TaskDefinition Modify(TaskDefinition taskDefinition);
    TaskDefinition ToTaskDefinition();
  }
}