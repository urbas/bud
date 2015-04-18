using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction {
  public abstract class ConfigModifier : IValueModifier<ConfigKey, IConfigDefinition> {
    public ConfigKey Key { get; }

    protected ConfigModifier(ConfigKey key) {
      Key = key;
    }

    public abstract void Modify(IDictionary<ConfigKey, IConfigDefinition> buildConfigurationBuilder);
  }
}