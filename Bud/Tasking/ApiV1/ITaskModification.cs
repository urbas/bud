namespace Bud.Tasking.ApiV1 {
  public interface ITaskModification {
    string Name { get; }
    ITaskDefinition Modify(ITaskDefinition taskDefinition);
    ITaskDefinition ToTaskDefinition();
  }
}