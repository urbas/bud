using System;
using System.Collections.Immutable;
using Bud.SettingsConstruction.Ops;

namespace Bud.SettingsConstruction.Builders {
  public class ConfigEnsureInitializedBuilder<T> : ConfigBuilder<T> {
    public ConfigEnsureInitializedBuilder(Settings existingSettings, ConfigKey<T> key) : base(existingSettings, key) {
    }

    public Settings OrInitializeWith(T value) {
      return ExistingSettings.Add(new SettingEnsureInitialization<T>(Key, value));
    }
  }

}

