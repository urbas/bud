using System;
using System.Collections.Immutable;
using Bud.SettingsConstruction.Builders;

namespace Bud.Settings {
  public static class SettingsUtils {
    public static ConfigEnsureInitializedBuilder<T> EnsureInitialized<T>(this ImmutableList<Setting> existingSettings, ConfigKey<T> key) {
      return new ConfigEnsureInitializedBuilder<T>(existingSettings, key);
    }

    public static ConfigModificationBuilder<T> Modify<T>(this ImmutableList<Setting> existingSettings, ConfigKey<T> key) {
      return new ConfigModificationBuilder<T>(existingSettings, key);
    }
  }
}

