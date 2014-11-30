using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction {
  public abstract class Setting {
    public readonly SettingKey Key;

    public Setting(SettingKey key) {
      this.Key = key;
    }

    public abstract void ApplyTo(ImmutableDictionary<ISettingKey, IValueDefinition>.Builder buildConfigurationBuilder);
  }
}

