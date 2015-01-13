namespace Bud {
  public static class PluginUtils {
    public static IPlugin With(this IPlugin thisPlugin, IPlugin otherPlugin) {
      return otherPlugin == null ? thisPlugin : thisPlugin.With(otherPlugin.ApplyTo);
    }

    public static IPlugin With(this IPlugin thisPlugin, params IPlugin[] otherPlugins) {
      IPlugin composedPlugin = thisPlugin;
      foreach (var otherPlugin in otherPlugins) {
        composedPlugin = composedPlugin.With(otherPlugin);
      }
      return composedPlugin;
    }

    public static IPlugin With(this IPlugin thisPlugin, SettingsTransform settingApplication) {
      return new SettingsTransformPlugin((existingSettings, key) => settingApplication(thisPlugin.ApplyTo(existingSettings, key), key));
    }

    public static IPlugin ApplyTo(Key key, IPlugin plugin) {
      return Create((context, oldKey) => plugin.ApplyTo(context, key));
    }

    public static IPlugin ApplyToSubKey(Key subKey, IPlugin plugin) {
      return Create((context, key) => plugin.ApplyTo(context, subKey.In(key)));
    }

    public static IPlugin Create(SettingsTransform settingApplication) {
      return new SettingsTransformPlugin(settingApplication);
    }

    public static readonly IPlugin Empty = new SettingsTransformPlugin();
  }
}