using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Builders {
  public class ConfigBuilder<T> {
    public readonly ImmutableList<Setting> ExistingSettings;
    public readonly ConfigKey<T> Key;

    public ConfigBuilder(ImmutableList<Setting> existingSettings, ConfigKey<T> key) {
      this.Key = key;
      this.ExistingSettings = existingSettings;
    }
  }
}

