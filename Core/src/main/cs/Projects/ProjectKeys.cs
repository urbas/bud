using System.Collections.Immutable;

namespace Bud.Projects {
  public static class ProjectKeys {
    public static readonly Key Project = new Key("project");
    public static readonly ConfigKey<ImmutableDictionary<string, Key>> Projects = new ConfigKey<ImmutableDictionary<string, Key>>("projects", Key.Root);
  }
}