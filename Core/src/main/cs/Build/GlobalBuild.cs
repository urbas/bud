using System.IO;
using Bud.Dependencies;

namespace Bud.Build {
  public static class GlobalBuild {
    public static Settings New(string globalBuildDir = ".") {
      return Settings.Create(BuildDirs.Init(globalBuildDir),
                             DependenciesPlugin.Init,
                             BuildDirsKeys.BudDir.Modify(GetDefaultGlobalBudDir));
    }

    private static string GetDefaultGlobalBudDir(IConfig ctxt) {
      return Path.Combine(ctxt.GetBaseDir(), BuildDirs.BudDirName, "global");
    }
  }
}