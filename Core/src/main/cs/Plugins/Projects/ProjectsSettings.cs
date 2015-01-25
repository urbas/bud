using System;

namespace Bud.Plugins.Projects {
  public static class ProjectsSettings {
    public static Settings Project(this Settings build, string id, string baseDir, params Func<Settings, Settings>[] plugins) {
      var projectKey = ProjectKey(id);
      return build
        .In(projectKey, new ProjectPlugin(id, baseDir).ApplyTo, plugins);
    }

    public static Key ProjectKey(string id) {
      return new Key(id).In(ProjectKeys.Project);
    }
  }
}