using System.Collections.Immutable;
using NuGet;

namespace Bud.Projects {
  public static class ProjectKeys {
    public static readonly Key Project = Key.Define("project");
    public static readonly ConfigKey<ImmutableDictionary<string, Key>> Projects = Key.Root / "projects";
    public static readonly ConfigKey<SemanticVersion> Version = Key.Define("version");
  }
}