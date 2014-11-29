using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud {
  public abstract class Setting {
    public readonly SettingKey Key;

    public Setting(SettingKey key) {
      this.Key = key;
    }

    public abstract void ApplyTo(ImmutableDictionary<ISettingKey, object>.Builder buildConfigurationBuilder);
  }
}

