using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public class SettingEnsureInitialization<T> : Setting {
    public T InitialValue;

    public SettingEnsureInitialization(SettingKey key, T initialValue) : base(key) {
      this.InitialValue = initialValue;
    }
  }
}

