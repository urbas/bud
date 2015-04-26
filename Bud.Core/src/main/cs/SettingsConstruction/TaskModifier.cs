using System.Collections.Generic;

namespace Bud.SettingsConstruction {
  public abstract class TaskModifier : IValueModifier<TaskKey, ITaskDefinition> {
    public TaskKey Key { get; }

    protected TaskModifier(TaskKey key) {
      Key = key;
    }

    public abstract void Modify(IDictionary<TaskKey, ITaskDefinition> taskDefinitionBuilder);
  }
}