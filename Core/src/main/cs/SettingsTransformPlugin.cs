namespace Bud {
  public class SettingsTransformPlugin : IPlugin {
    public readonly SettingsTransform SettingsTransform;
    public static readonly SettingsTransform IdentitySettingsTransform = existingSettings => existingSettings;

    internal SettingsTransformPlugin() : this(IdentitySettingsTransform) {}

    public SettingsTransformPlugin(SettingsTransform settingsTransform) {
      SettingsTransform = settingsTransform;
    }

    public Settings ApplyTo(Settings settings) {
      return SettingsTransform(settings);
    }
  }
}