using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Util {
  public static class SettingsUtils {
    public static IDictionary<Scope, IValueDefinition> ToCompiledSettings(Settings settings) {
      var buildConfigurationBuilder = ImmutableDictionary.CreateBuilder<Scope, IValueDefinition>();
      foreach (var setting in settings.SettingsList) {
        setting.ApplyTo(buildConfigurationBuilder);
      }
      return buildConfigurationBuilder.ToImmutable();
    }
  }
}

