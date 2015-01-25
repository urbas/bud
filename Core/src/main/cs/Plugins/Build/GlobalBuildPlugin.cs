using System;
using System.IO;

namespace Bud.Plugins.Build {
  public class GlobalBuildPlugin {
    public static Func<Settings, Settings> Init(string baseDir) {
      return settings => settings.Do(
          BuildDirsPlugin.Init(baseDir),
          BuildDirsKeys.BudDir.Modify(ctxt => Path.Combine(ctxt.GetBaseDir(), BuildDirs.BudDirName, "global"))
        );
    }
  }
}