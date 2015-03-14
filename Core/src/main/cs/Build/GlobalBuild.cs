using System.IO;
using Bud.Dependencies;
using Bud.Projects;
using Bud.Publishing;
using NuGet;

namespace Bud.Build {
  public static class GlobalBuild {
    public static Settings New(string globalBuildDir = ".") {
      return Settings.Create(BuildDirs.Init(globalBuildDir),
                             DependenciesPlugin.Init,
                             BuildDirsKeys.BudDir.Modify(GetDefaultGlobalBudDir),
                             ProjectKeys.Version.Init(GlobalBuildSettings.DefaultBuildVersion),
                             PublishingPlugin.InitGlobally());
    }

    private static string GetDefaultGlobalBudDir(IConfig ctxt) {
      return Path.Combine(ctxt.GetBaseDir(), BudPaths.BudDirName, "global");
    }
  }

  public class GlobalBuildSettings {
    public static readonly SemanticVersion DefaultBuildVersion = SemanticVersion.Parse("0.0.1");
  }
}