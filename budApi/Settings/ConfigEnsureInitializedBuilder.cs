using System;
using System.Collections.Immutable;

namespace Bud.Settings {

  public class ConfigEnsureInitializedBuilder<T> : ConfigBuilder<T> {

    public ConfigEnsureInitializedBuilder(ImmutableList<Setting> existingSettings, ConfigKey<T> key): base(existingSettings, key) { }

    public ImmutableList<Setting> OrInitializeWith(T value) {
      return ExistingSettings.Add(new SettingEnsureInitialization(Key, new SettingConstantValue<T>(value)));
    }
  }

}

