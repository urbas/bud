using System.Collections.Immutable;
using NuGet;

namespace Bud.Projects {
  public static class ProjectsConfig {
    public static ImmutableDictionary<string, Key> GetAllProjects(this IConfig buildConfiguration) {
      return buildConfiguration.Evaluate(ProjectKeys.Projects);
    }

    public static Key GetProject(this IConfig context, string id) {
      return GetAllProjects(context)[id];
    }

    public static SemanticVersion GetVersionOf(this IConfig context, Key project) {
      return context.Evaluate(project / ProjectKeys.Version);
    }
  }
}