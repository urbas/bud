using System.Collections.Immutable;

namespace Bud.SettingsConstruction {
  public abstract class TaskDefinitionConstructor {
    public readonly TaskKey Key;

    protected TaskDefinitionConstructor(TaskKey key) {
      Key = key;
    }

    public abstract void ApplyTo(ImmutableDictionary<Key, ITaskDefinition>.Builder taskDefinitionBuilder);
  }
}