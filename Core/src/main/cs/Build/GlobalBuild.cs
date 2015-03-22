using System.IO;
using Bud.Dependencies;
using Bud.Projects;
using Bud.Publishing;

namespace Bud.Build {
  public static class GlobalBuild {
    public const string GlobalBudDirName = "global";

    public static Settings New(string globalBuildDir = ".") {
      return Settings.Create(new BuildDirsPlugin(globalBuildDir),
                             DependenciesPlugin.Init,
                             BuildDirsKeys.BudDir.Modify(GetDefaultGlobalBudDir),
                             ProjectKeys.Version.Init(GlobalBuildSettings.DefaultBuildVersion));
    }

    private static string GetDefaultGlobalBudDir(IConfig ctxt) {
      return Path.Combine(ctxt.GetBaseDir(), BudPaths.BudDirName, GlobalBudDirName);
    }
  }
}