using System.IO;
using System.Collections.Immutable;
using System.Collections.Generic;
using Bud.Plugins.Dependencies;

namespace Bud.Plugins.Projects {

  public static class Project {

    public static Settings AddProject(this Settings build, string id, string baseDir, IPlugin plugin = null) {
      var projectScope = ProjectScope(id);
      return build
        .Apply(projectScope, new ProjectPlugin(id, baseDir))
        .Apply(projectScope, plugin ?? Plugin.New);
    }

    public static IPlugin Dependency(string projectId) {
      return ProjectScope(projectId).ToDependency();
    }

    public static Key ProjectScope(string id) {
      return new Key(id).In(ProjectKeys.Project);
    }

    public static ImmutableDictionary<string, Key> GetAllProjects(this IConfiguration buildConfiguration) {
      return buildConfiguration.Evaluate(ProjectKeys.Projects);
    }

    public static Key GetProject(this IConfiguration context, string id) {
      return GetAllProjects(context)[id];
    }
  }

}

