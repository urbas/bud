using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction {
  public abstract class Setting {
    public readonly Scope Key;

    public Setting(Scope key) {
      this.Key = key;
    }

    public abstract void ApplyTo(ImmutableDictionary<Scope, IValueDefinition>.Builder buildConfigurationBuilder);
  }
}

