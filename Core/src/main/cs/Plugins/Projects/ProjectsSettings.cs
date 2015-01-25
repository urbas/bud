using System;

namespace Bud.Plugins.Projects {
  public static class ProjectsSettings {
    public static Settings Project(this Settings settings, string id, string baseDir, params Func<Settings, Settings>[] plugins) {
      return settings.Do(ProjectPlugin.Init(id, baseDir, plugins));
    }

    public static Key ProjectKey(string id) {
      return new Key(id).In(ProjectKeys.Project);
    }
  }
}