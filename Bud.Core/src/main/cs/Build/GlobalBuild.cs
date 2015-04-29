using System.IO;
using Bud.BuildDefinition;
using Bud.Dependencies;
using Bud.Projects;

namespace Bud.Build {
  public static class GlobalBuild {
    public const string GlobalBudDirName = "global";

    public static Settings New(string globalBuildDir = ".") {
      return Settings.Create(new BuildDirsPlugin(globalBuildDir),
                             DependenciesPlugin.Instance,
                             BuildDirsKeys.BudDir.Modify(GetDefaultGlobalBudDir),
                             ProjectKeys.Version.Init(GlobalBuildSettings.DefaultBuildVersion));
    }

    private static string GetDefaultGlobalBudDir(IConfig ctxt) {
      return Path.Combine(ctxt.GetBaseDir(), BudPaths.BudDirName, GlobalBudDirName);
    }
  }
}