using System.IO;
using Bud.BuildDefinition;
using Bud.BuildDefinition.BuildDefinitionPlugin;
using Bud.BuildDefinition.BuildDefinitionSettings;

namespace Bud.Commander {
  public static class BuildCommander {
    public static IBuildCommander LoadBuildCommander(int buildLevel,
                                                     string currentProjectBaseDir) {
      // TODO: descend to the lowest build level first (i.e., go to the directory .bud/.../.bud/)
      if (buildLevel <= 0) {
        return LoadProjectLevelCommander(currentProjectBaseDir);
      }
      return LoadBuildLevelCommander(currentProjectBaseDir);
    }

    public static IBuildCommander LoadProjectLevelCommander(string projectBaseDir) {
      var buildDefinitionProject = DefaultBuildDefinitionProject(projectBaseDir);
      var context = Context.FromSettings(buildDefinitionProject);
      var buildCommanderTask = context.CreateBuildCommander(BuildDefinitionProjectKey);
      buildCommanderTask.Wait();
      return buildCommanderTask.Result;
    }

    public static IBuildCommander LoadBuildLevelCommander(string projectBaseDir) {
      return new DefaultBuildCommander(DefaultBuildDefinitionProject(projectBaseDir));
    }
  }
}