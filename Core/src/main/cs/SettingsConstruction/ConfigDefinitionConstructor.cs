using System.Collections.Immutable;

namespace Bud.SettingsConstruction {
  public abstract class ConfigDefinitionConstructor {
    public readonly ConfigKey Key;

    protected ConfigDefinitionConstructor(ConfigKey key) {
      Key = key;
    }

    public abstract void ApplyTo(ImmutableDictionary<Key, IConfigDefinition>.Builder buildConfigurationBuilder);
  }
}