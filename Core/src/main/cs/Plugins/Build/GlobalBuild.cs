using System.IO;

namespace Bud.Plugins.Build {
  public static class GlobalBuild {
    public static Settings New(string globalBuildDir = ".") {
      return Settings.Create(Init(globalBuildDir));
    }

    private static Setup Init(string baseDir) {
      return settings => settings.Do(
        BuildDirs.Init(baseDir),
        BuildDirsKeys.BudDir.Modify(GetDefaultGlobalBudDir));
    }

    private static string GetDefaultGlobalBudDir(IConfig ctxt) {
      return Path.Combine(ctxt.GetBaseDir(), BuildDirs.BudDirName, "global");
    }
  }
}