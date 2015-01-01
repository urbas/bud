using System;

namespace Bud {
  public class Plugin : IPlugin {
    public static readonly Plugin New = new Plugin();

    public readonly SettingApplication SettingApplication;

    private Plugin() : this((existingSettings, scope) => existingSettings) {}

    public Plugin(SettingApplication settingApplication) {
      SettingApplication = settingApplication;
    }

    public static Plugin Create(SettingApplication settingApplication) {
      return new Plugin(settingApplication);
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return SettingApplication(settings, scope);
    }
	}
}

