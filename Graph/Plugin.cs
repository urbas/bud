using System;

namespace Bud {
  public class Plugin : IPlugin {
    public static readonly Plugin New = new Plugin();

    public readonly SettingsTransform SettingsTransform;

    private Plugin() : this((existingSettings, key) => existingSettings) {}

    public Plugin(SettingsTransform settingsTransform) {
      SettingsTransform = settingsTransform;
    }

    public static Plugin Create(SettingsTransform settingApplication) {
      return new Plugin(settingApplication);
    }

    public Settings ApplyTo(Settings settings, Key project) {
      return SettingsTransform(settings, project);
    }
	}
}

