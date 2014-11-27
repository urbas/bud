using System;
using System.Collections.Immutable;

namespace Bud.Settings.Construction {
  public class ConfigModificationBuilder<T> : ConfigBuilder<T> {

    public ConfigModificationBuilder(ImmutableList<Setting> existingSettings, ConfigKey<T> key): base(existingSettings, key) { }

    public ImmutableList<Setting> ByMapping(Func<T, T> value) {
      return ExistingSettings.Add(new ConfigModification<T>(Key, new ConfigValueModification<T>(value)));
    }
  }
}

