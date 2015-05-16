using System;
using System.Collections.Generic;

namespace Bud.SettingsConstruction {
  public class ModifyConfig<T> : ConfigModifier {
    private readonly Func<IConfig, T, T> ValueModifier;

    public ModifyConfig(ConfigKey key, Func<IConfig, T, T> valueModifier) : base(key) {
      ValueModifier = valueModifier;
    }

    public override void Modify(IDictionary<ConfigKey, IConfigDefinition> buildConfigurationBuilder) {
      IConfigDefinition value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        ConfigDefinition<T> existingValue = (ConfigDefinition<T>) value;
        buildConfigurationBuilder[Key] = new ConfigDefinition<T>(b => ValueModifier(b, existingValue.Evaluate(b)));
      } else {
        throw new InvalidOperationException($"Cannot modify the value of config key '{Key}'. This key has not yet been initialised.");
      }
    }
  }
}