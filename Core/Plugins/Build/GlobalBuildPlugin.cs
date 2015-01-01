using System;
using System.IO;

namespace Bud.Plugins.Build {

  public class GlobalBuildPlugin : IPlugin {
    private readonly string baseDir;

    public GlobalBuildPlugin(string baseDir) {
      this.baseDir = baseDir;
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .InitOrKeep(GlobalBuildKeys.GlobalBaseDir, baseDir)
        .InitOrKeep(GlobalBuildKeys.GlobalBudDir, ctxt => Path.Combine(ctxt.GetGlobalBaseDir(), ".bud", "global"))
        .InitOrKeep(GlobalBuildKeys.GlobalBuildConfigCacheDir, ctxt => Path.Combine(ctxt.GetGlobalBudDir(), "buildConfigCache"));
    }
  }
}

