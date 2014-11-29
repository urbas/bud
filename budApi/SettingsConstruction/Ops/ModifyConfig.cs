using System;
using System.Collections.Immutable;
using Bud;

namespace Bud.SettingsConstruction.Ops {
  public class ModifyConfig<T> : Setting {
    Func<BuildConfiguration, T, T> ValueModifier;

    public ModifyConfig(ConfigKey<T> key, Func<T, T> valueModifier) : this(key, (buildConfig, previousValue) => valueModifier(previousValue)) {}

    public ModifyConfig(ConfigKey<T> key, Func<BuildConfiguration, T, T> valueModifier) : base(key) {
      this.ValueModifier = valueModifier;
    }

    public override void ApplyTo(ImmutableDictionary<ISettingKey, IValueDefinition>.Builder buildConfigurationBuilder) {
      IValueDefinition value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        IValueDefinition<T> existingValue = (IValueDefinition<T>)value;
        buildConfigurationBuilder[Key] = new ConfigModification<T>(existingValue, ValueModifier);
      } else {
        throw new InvalidOperationException(string.Format("Cannot modify the value of key '{0}'. This key has not yet been initialised.", Key.GetType().FullName));
      }
    }
  }
}

