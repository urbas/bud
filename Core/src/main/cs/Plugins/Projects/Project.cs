using System.IO;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using Bud.Plugins.Build;
using Bud.Plugins.Deps;
using System.Threading.Tasks;

namespace Bud.Plugins.Projects {

  public static class Project {

    public static Settings AddProject(this Settings build, string id, string baseDir, IPlugin plugin = null) {
      var projectKey = ProjectKey(id);
      return build
        .Apply(projectKey, new ProjectPlugin(id, baseDir).With(plugin));
    }

    public static Key ProjectKey(string id) {
      return new Key(id).In(ProjectKeys.Project);
    }

    public static ImmutableDictionary<string, Key> GetAllProjects(this IConfig buildConfiguration) {
      return buildConfiguration.Evaluate(ProjectKeys.Projects);
    }

    public static Key GetProject(this IConfig context, string id) {
      return GetAllProjects(context)[id];
    }
  }

}

