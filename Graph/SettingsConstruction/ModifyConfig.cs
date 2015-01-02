using System;
using System.Collections.Immutable;
using Bud;

namespace Bud.SettingsConstruction {
  public class ModifyConfig<T> : ConfigDefinitionConstructor {
    Func<Configuration, T, T> ValueModifier;

    public ModifyConfig(ConfigKey<T> key, Func<Configuration, T, T> valueModifier) : base(key) {
      this.ValueModifier = valueModifier;
    }

    public override void ApplyTo(ImmutableDictionary<Key, IConfigDefinition>.Builder buildConfigurationBuilder) {
      IConfigDefinition value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        ConfigDefinition<T> existingValue = (ConfigDefinition<T>)value;
        buildConfigurationBuilder[Key] = new ConfigDefinition<T>(b => ValueModifier(b, existingValue.Evaluate(b)));
      } else {
        throw new InvalidOperationException(string.Format("Cannot modify the value of key '{0}'. This key has not yet been initialised.", Key.GetType().FullName));
      }
    }
  }
}

