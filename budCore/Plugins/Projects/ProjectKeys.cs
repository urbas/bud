using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Projects {
  public static class ProjectKeys {
    public static readonly Scope Project = new Scope("Projects");
    public static readonly ConfigKey<ImmutableHashSet<Scope>> ListOfProjects = new ConfigKey<ImmutableHashSet<Scope>>("ListOfProjects");
    public static readonly ConfigKey<string> BaseDir = new ConfigKey<string>("BaseDir");
    public static readonly ConfigKey<string> BudDir = new ConfigKey<string>("BudDir");
    public static readonly ConfigKey<string> OutputDir = new ConfigKey<string>("OutputDir");
    public static readonly ConfigKey<string> BuildConfigCacheDir = new ConfigKey<string>("BuildConfigCacheDir");
  }
}

