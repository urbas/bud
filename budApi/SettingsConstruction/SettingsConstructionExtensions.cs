using System;
using System.Collections.Immutable;
using Bud.SettingsConstruction.Ops;

namespace Bud.SettingsConstruction {
  public static class SettingsConstructionExtensions {
    public static Settings EnsureInitialized<T>(this Settings existingSettings, ConfigKey<T> key, T initialValue) {
      return existingSettings.Add(EnsureConfigInitialized.Create(key, initialValue));
    }

    public static Settings Modify<T>(this Settings existingSettings, ConfigKey<T> key, Func<T, T> modifier) {
      return existingSettings.Add(ModifyConfig.Create(key, modifier));
    }
  }
}

