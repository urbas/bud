using System.Collections.Immutable;
using NuGet;

namespace Bud.Projects {
  public static class ProjectKeys {
    public static readonly Key Project = new Key("project");
    public static readonly ConfigKey<ImmutableDictionary<string, Key>> Projects = new ConfigKey<ImmutableDictionary<string, Key>>("projects", Key.Root);
    public static readonly ConfigKey<SemanticVersion> Version = new ConfigKey<SemanticVersion>("version");
  }
}