using System;

namespace Bud {
  public class Plugin : IPlugin {
    public static readonly Plugin New = new Plugin();

    public readonly SettingsTransform SettingApplication;

    private Plugin() : this((existingSettings, scope) => existingSettings) {}

    public Plugin(SettingsTransform settingApplication) {
      SettingApplication = settingApplication;
    }

    public static Plugin Create(SettingsTransform settingApplication) {
      return new Plugin(settingApplication);
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return SettingApplication(settings, scope);
    }
	}
}

