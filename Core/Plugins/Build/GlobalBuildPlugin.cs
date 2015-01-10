using System;
using System.IO;

namespace Bud.Plugins.Build {

  public class GlobalBuildPlugin : IPlugin {
    private readonly string baseDir;

    public GlobalBuildPlugin(string baseDir) {
      this.baseDir = baseDir;
    }

    public Settings ApplyTo(Settings settings, Key project) {
      return settings
        .Apply(Key.Global, new BuildDirsPlugin(baseDir))
        .Modify(BuildDirsKeys.BudDir, (ctxt, oldValue) => Path.Combine(ctxt.GetBaseDir(), BuildDirs.BudDirName, "global"));
    }
  }
}

