using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Build {
  public static class GlobalBuildKeys {
    public static readonly ConfigKey<string> GlobalBaseDir = new ConfigKey<string>("GlobalBaseDir");
    public static readonly ConfigKey<string> GlobalBudDir = new ConfigKey<string>("GlobalBudDir");
    public static readonly ConfigKey<string> GlobalBuildConfigCacheDir = new ConfigKey<string>("GlobalBuildConfigCacheDir");
  }
}

