using System.Collections.Immutable;

namespace Bud.SettingsConstruction {
  public abstract class ConfigDefinitionConstructor {
    public readonly ConfigKey Key;

    public ConfigDefinitionConstructor(ConfigKey key) {
      Key = key;
    }

    public abstract void ApplyTo(ImmutableDictionary<Key, IConfigDefinition>.Builder buildConfigurationBuilder);
  }
}