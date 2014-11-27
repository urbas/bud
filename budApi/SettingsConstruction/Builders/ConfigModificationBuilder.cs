using System;
using System.Collections.Immutable;
using Bud.SettingsConstruction.Ops;

namespace Bud.SettingsConstruction.Builders {
  public class ConfigModificationBuilder<T> : ConfigBuilder<T> {

    public ConfigModificationBuilder(Settings existingSettings, ConfigKey<T> key): base(existingSettings, key) { }

    public Settings ByMapping(Func<T, T> value) {
      return ExistingSettings.Add(new ConfigModification<T>(Key, value));
    }
  }
}

