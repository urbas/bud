namespace Bud.Plugins.Projects {
  public static class ProjectsSettings {
    public static Settings Project(this Settings build, string id, string baseDir, params IPlugin[] plugins) {
      var projectKey = ProjectKey(id);
      return build
        .Apply(projectKey, new ProjectPlugin(id, baseDir).With(plugins));
    }

    public static Key ProjectKey(string id) {
      return new Key(id).In(ProjectKeys.Project);
    }
  }
}