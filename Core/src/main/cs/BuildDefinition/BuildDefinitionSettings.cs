using System.IO;
using Bud.Build;
using Bud.Projects;

namespace Bud.BuildDefinition {
  public static class BuildDefinitionSettings {
    public static Settings DefaultBuildDefinitionProject(string defineeProjectBaseDir) {
      var buildProjectDir = Path.Combine(defineeProjectBaseDir, BudPaths.BudDirName);
      return GlobalBuild.New(buildProjectDir)
                        .Project(BuildDefinitionPlugin.BuildDefinitionProjectId, buildProjectDir, BuildDefinitionPlugin.AddToProject(defineeProjectBaseDir));
    }
  }
}