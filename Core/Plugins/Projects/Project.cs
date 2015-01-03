using System.IO;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using Bud.Plugins.Build;
using Bud.Plugins.Dependencies;
using System.Threading.Tasks;

namespace Bud.Plugins.Projects {

  public static class Project {

    public static Settings AddProject(this Settings build, string id, string baseDir, IPlugin plugin = null) {
      var projectKey = ProjectKey(id);
      return build
        .Apply(projectKey, new ProjectPlugin(id, baseDir))
        .Apply(projectKey, plugin ?? Plugin.New);
    }

    public static Key ProjectKey(string id) {
      return new Key(id).In(ProjectKeys.Project);
    }

    public static IPlugin Dependency(string projectId) {
      return Bud.Plugins.Dependencies.Dependencies.Add(BuildKeys.Build, BuildKeys.Build.In(ProjectKey(projectId)));
    }

    /// <returns>The projects that have been built.</returns>
    public static async Task<ImmutableList<Key>> ResolveBuildDependencies(this IEvaluationContext context, Key project) {
      var resolvedBuilds = await context.ResolveDependencies(project, BuildKeys.Build);
      return resolvedBuilds.Select(build => build.Parent).ToImmutableList();
    }

    public static ImmutableDictionary<string, Key> GetAllProjects(this IConfiguration buildConfiguration) {
      return buildConfiguration.Evaluate(ProjectKeys.Projects);
    }

    public static Key GetProject(this IConfiguration context, string id) {
      return GetAllProjects(context)[id];
    }
  }

}

