using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Builders {
  public class ConfigBuilder<T> {
    public readonly Settings ExistingSettings;
    public readonly ConfigKey<T> Key;

    public ConfigBuilder(Settings existingSettings, ConfigKey<T> key) {
      this.Key = key;
      this.ExistingSettings = existingSettings;
    }
  }
}

