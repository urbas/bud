using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Util {
  public static class SettingsUtils {
    public static IDictionary<ISettingKey, IValueDefinition> ToCompiledSettings(Settings settings) {
      var buildConfigurationBuilder = ImmutableDictionary.CreateBuilder<ISettingKey, IValueDefinition>();
      foreach (var setting in settings.SettingsList) {
        setting.ApplyTo(buildConfigurationBuilder);
      }
      return buildConfigurationBuilder.ToImmutable();
    }
  }
}

