using System.Collections.Immutable;

namespace Bud.Projects {
  public static class ProjectsConfig {
    public static ImmutableDictionary<string, Key> GetAllProjects(this IConfig buildConfiguration) {
      return buildConfiguration.Evaluate(ProjectKeys.Projects);
    }

    public static Key GetProject(this IConfig context, string id) {
      return GetAllProjects(context)[id];
    }
  }
}