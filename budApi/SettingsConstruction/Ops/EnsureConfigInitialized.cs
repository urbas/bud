using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {

  public static class EnsureConfigInitialized {
    public static EnsureConfigInitialized<T> Create<T>(ConfigKey<T> key, T initialValue) {
      return new EnsureConfigInitialized<T>(key, initialValue);
    }
  }

  public class EnsureConfigInitialized<T> : Setting {
    public T InitialValue;

    public EnsureConfigInitialized(ConfigKey<T> key, T initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<ISettingKey, object>.Builder buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = InitialValue;
      }
    }
  }
}

