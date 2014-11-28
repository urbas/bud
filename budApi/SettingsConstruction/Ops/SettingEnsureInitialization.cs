using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {

  public static class SettingEnsureInitialization {
    public static SettingEnsureInitialization<T> Create<T>(SettingKey key, T initialValue) {
      return new SettingEnsureInitialization<T>(key, initialValue);
    }
  }

  public class SettingEnsureInitialization<T> : Setting {
    public T InitialValue;

    public SettingEnsureInitialization(SettingKey key, T initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<SettingKey, object>.Builder buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = InitialValue;
      }
    }
  }
}

