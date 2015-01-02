using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Build {
  public static class BuildDirsKeys {
    public static readonly ConfigKey<string> BaseDir = new ConfigKey<string>("BaseDir");
    public static readonly ConfigKey<string> BudDir = new ConfigKey<string>("BudDir");
    public static readonly ConfigKey<string> OutputDir = new ConfigKey<string>("OutputDir");
    public static readonly ConfigKey<string> BuildConfigCacheDir = new ConfigKey<string>("BuildConfigCacheDir");
    public static readonly ConfigKey<string> PersistentBuildConfigDir = new ConfigKey<string>("PersistentBuildConfigDir");
  }
}

