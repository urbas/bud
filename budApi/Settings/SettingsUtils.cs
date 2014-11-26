using System;
using System.Collections.Immutable;

namespace Bud.Settings {
  public static class SettingsUtils {
    public static readonly ImmutableList<Setting> Start = ImmutableList.Create<Setting>();

    public static SettingInitializationBuilder InitializeSetting(this ImmutableList<Setting> existingSettings, SettingKey key) {
      return new SettingInitializationBuilder(existingSettings, key);
    }
  }
}

