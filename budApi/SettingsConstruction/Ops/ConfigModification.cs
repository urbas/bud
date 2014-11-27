using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public class ConfigModification<T> : Setting {
    Func<T, T> ValueModifier;

    public ConfigModification(ConfigKey<T> key, Func<T, T> valueModifier) : base(key) {
      this.ValueModifier = valueModifier;
    }
  }
}

