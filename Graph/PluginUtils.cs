using System;

namespace Bud {
  public static class PluginUtils {
    public static IPlugin With(this IPlugin thisPlugin, IPlugin otherPlugin) {
      return otherPlugin == null ? thisPlugin : thisPlugin.With(otherPlugin.ApplyTo);
    }

    public static IPlugin With(this IPlugin thisPlugin, SettingsTransform settingApplication) {
      return new Plugin((existingSettings, key) => settingApplication(thisPlugin.ApplyTo(existingSettings, key), key));
    }
  }
}

