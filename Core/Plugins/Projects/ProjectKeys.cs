using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Projects {
  public static class ProjectKeys {
    public static readonly Key Project = new Key("Project");
    public static readonly ConfigKey<ImmutableDictionary<string, Key>> Projects = new ConfigKey<ImmutableDictionary<string, Key>>("Projects");
  }
}

