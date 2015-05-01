using System.IO;
using Bud.BuildDefinition;
using Bud.Cli.Macros;
using Bud.Dependencies;
using Bud.Projects;

namespace Bud.Build {
  public static class GlobalBuild {
    public const string GlobalBudDirName = "global";

    public static Settings New(string globalBuildDir = ".") {
      return Settings.Create(new BuildDirsPlugin(globalBuildDir),
                             DependenciesPlugin.Instance,
                             BuildDirsKeys.BudDir.Modify(GetDefaultGlobalBudDir),
                             ProjectKeys.Version.Init(GlobalBuildSettings.DefaultBuildVersion),
                             new Macro("interactiveMode", InteractiveModeMacro.InteractiveModeMacroImpl, "Executes Bud in the interactive command line mode."));
    }

    private static string GetDefaultGlobalBudDir(IConfig ctxt) {
      return Path.Combine(ctxt.GetBaseDir(), BudPaths.BudDirName, GlobalBudDirName);
    }
  }
}