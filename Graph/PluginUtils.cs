using System;

namespace Bud {
  public static class PluginUtils {
    public static IPlugin With(this IPlugin thisPlugin, IPlugin otherPlugin) {
      return otherPlugin == null ? thisPlugin : thisPlugin.With(otherPlugin.ApplyTo);
    }

    public static IPlugin With(this IPlugin thisPlugin, SettingApplication settingApplication) {
      return new Plugin((existingSettings, scope) => settingApplication(thisPlugin.ApplyTo(existingSettings, scope), scope));
    }
  }
}

