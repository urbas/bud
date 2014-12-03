using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Projects {
  public static class ProjectKeys {
    public static readonly Scope Project = new Scope("Project");
    public static readonly ConfigKey<ImmutableDictionary<string, Scope>> Projects = new ConfigKey<ImmutableDictionary<string, Scope>>("Projects");
  }
}

