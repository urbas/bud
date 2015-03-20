using System.Collections.Immutable;

namespace Bud.SettingsConstruction {
  public abstract class TaskModifier {
    public readonly TaskKey Key;

    protected TaskModifier(TaskKey key) {
      Key = key;
    }

    public abstract void ApplyTo(ImmutableDictionary<Key, ITaskDefinition>.Builder taskDefinitionBuilder);
  }
}