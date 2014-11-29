using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public static class ModifyConfig {
    public static Setting Create<T>(ConfigKey<T> key, Func<T, T> valueModifier) {
      return new ModifyConfig<T>(key, valueModifier);
    }
  }

  public class ModifyConfig<T> : Setting {
    Func<T, T> ValueModifier;

    public ModifyConfig(ConfigKey<T> key, Func<T, T> valueModifier) : base(key) {
      this.ValueModifier = valueModifier;
    }

    public override void ApplyTo(ImmutableDictionary<ISettingKey, object>.Builder buildConfigurationBuilder) {
      object value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        T existingValue = (T)value;
        buildConfigurationBuilder[Key] = ValueModifier(existingValue);
      } else {
        throw new InvalidOperationException(string.Format("Cannot modify the value of key '{0}'. This key has not yet been initialised.", Key.GetType().FullName));
      }
    }
  }
}

