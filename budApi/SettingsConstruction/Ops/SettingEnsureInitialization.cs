using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public class SettingEnsureInitialization<T> : Setting {
    public T InitialValue;

    public SettingEnsureInitialization(SettingKey key, T initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    #region implemented abstract members of Setting

    public override void ApplyTo(ImmutableDictionary<SettingKey, object>.Builder buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = InitialValue;
      }
    }

    #endregion
  }
}

