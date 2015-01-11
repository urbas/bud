namespace Bud {
  public class SettingsTransformPlugin : IPlugin {
    public readonly SettingsTransform SettingsTransform;

    internal SettingsTransformPlugin() : this((existingSettings, key) => existingSettings) {}

    public SettingsTransformPlugin(SettingsTransform settingsTransform) {
      SettingsTransform = settingsTransform;
    }

    public Settings ApplyTo(Settings settings, Key project) {
      return SettingsTransform(settings, project);
    }
  }
}