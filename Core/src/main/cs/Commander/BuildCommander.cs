using System.IO;
using Bud.Build;
using Bud.BuildDefinition;
using Bud.Logging;
using Bud.Projects;

namespace Bud.Commander {
  public static class BuildCommander {
    public static IBuildCommander LoadBuildCommander(int buildLevel,
                                                     string currentProjectBaseDir) {
      // TODO: descend to the lowest build level first (i.e., go to the directory .bud/.../.bud/)
//      return LoadBuildCommander(buildLevel, 0, currentProjectBaseDir);
      if (buildLevel <= 0) {
        return LoadProjectLevelCommander(currentProjectBaseDir);
      }
      return LoadBuildCommander(buildLevel - 1, Path.Combine(currentProjectBaseDir, BudPaths.BudDirName));
    }

//    private static IBuildCommander LoadBuildCommander(int targetBuildLevel, int currentBuildLevel, string currentProjectBaseDir) {
//      if (targetBuildLevel > currentBuildLevel) {
//        return Path.Combine(currentBuildLevel, BudPaths.GlobalConfigDir);
//      }
//    }

    public static IBuildCommander LoadProjectLevelCommander(string path) {
      var buildSettings = LoadBuildLevelSettings(path);
      var context = Context.FromSettings(buildSettings, Logger.CreateFromStandardOutputs());
      var buildCommanderTask = context.CreateBuildCommander(ProjectPlugin.ProjectKey(BuildDefinitionPlugin.BuildDefinitionProjectId));
      buildCommanderTask.Wait();
      return buildCommanderTask.Result;
    }

    /// <summary>
    ///   Loads build level 1.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static Settings LoadBuildLevelSettings(string path) {
      var buildProjectDir = Path.Combine(path, BudPaths.BudDirName);
      return GlobalBuild.New(buildProjectDir)
                        .Project(BuildDefinitionPlugin.BuildDefinitionProjectId, buildProjectDir, BuildDefinitionPlugin.AddToProject(path));
    }

    public static IBuildCommander LoadBuildLevelCommander(string projectLevelDir) {
      return new DefaultBuildCommander(LoadBuildLevelSettings(projectLevelDir));
    }
  }
}