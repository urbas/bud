using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;

namespace Bud.Plugins {

  public static class Project {
    public static ScopedSettings New(string id, string baseDir) {
      return Settings.Start.AddProject(id, baseDir);
    }

    public static SettingKey Key(string id) {
      return new SettingKey(id).In(ProjectPlugin.Projects);
    }
  }

}

