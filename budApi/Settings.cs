using System;
using System.Collections.Immutable;

namespace Bud {
  public class Settings {
    public static readonly Settings Start = new Settings(ImmutableList.Create<Setting>());

    public readonly ImmutableList<Setting> SettingsList;

    public Settings(ImmutableList<Setting> settings) {
      this.SettingsList = settings;
    }

    public Settings Add(Setting setting) {
      return new Settings(SettingsList.Add(setting));
    }

    public Settings Add(Settings settings) {
      return new Settings(SettingsList.AddRange(settings.SettingsList));
    }

    public BuildConfiguration Compile() {
      return new BuildConfiguration();
    }
  }
}