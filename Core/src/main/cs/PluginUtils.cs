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
      return new SettingsTransformPlugin(existingSettings => settingApplication(thisPlugin.ApplyTo(existingSettings)));
    }

    public static IPlugin ApplyTo(Key key, IPlugin plugin) {
      return Create(settings => settings.In(key, plugin.ApplyTo));
    }

    public static IPlugin ApplyToSubKey(Key subKey, IPlugin plugin) {
      return Create(context => context.In(subKey.In(context.Scope), plugin.ApplyTo));
    }

    public static IPlugin Create(SettingsTransform settingApplication) {
      return new SettingsTransformPlugin(settingApplication);
    }

    public static readonly IPlugin Empty = new SettingsTransformPlugin();
  }
}