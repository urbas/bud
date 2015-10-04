namespace Bud.Tasking {
  internal interface ITaskModification {
    string Name { get; }
    TaskDefinition Modify(TaskDefinition taskDefinition);
    TaskDefinition ToTaskDefinition();
  }
}