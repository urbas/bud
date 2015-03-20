using System.Collections.Immutable;

namespace Bud.SettingsConstruction {
  public abstract class ConfigModifier {
    public readonly ConfigKey Key;

    protected ConfigModifier(ConfigKey key) {
      Key = key;
    }

    public abstract void ApplyTo(ImmutableDictionary<Key, IConfigDefinition>.Builder buildConfigurationBuilder);
  }
}