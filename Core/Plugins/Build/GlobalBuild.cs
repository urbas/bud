using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Build {
  public static class GlobalBuild {
    public static Settings New(string globalBuildDir = ".") {
      return Settings.Empty.Apply(Key.Global, new GlobalBuildPlugin(globalBuildDir));
    }

    public static string GetGlobalBaseDir(this Configuration ctxt) {
      return ctxt.Evaluate(GlobalBuildKeys.GlobalBaseDir);
    }

    public static string GetGlobalBudDir(this Configuration ctxt) {
      return ctxt.Evaluate(GlobalBuildKeys.GlobalBudDir);
    }

    public static string GetGlobalBuildConfigCacheDir(this Configuration ctxt) {
      return ctxt.Evaluate(GlobalBuildKeys.GlobalBuildConfigCacheDir);
    }
  }
}

