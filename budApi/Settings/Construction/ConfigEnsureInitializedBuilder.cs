using System;
using System.Collections.Immutable;

namespace Bud.Settings.Construction {

  public class ConfigEnsureInitializedBuilder<T> : ConfigBuilder<T> {

    public ConfigEnsureInitializedBuilder(ImmutableList<Setting> existingSettings, ConfigKey<T> key): base(existingSettings, key) { }

    public ImmutableList<Setting> OrInitializeWith(T value) {
      return ExistingSettings.Add(new SettingEnsureInitialization(Key, new SettingConstantValue<T>(value)));
    }
  }

}

