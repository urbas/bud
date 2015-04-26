using Bud.Build;
using Bud.Projects;

namespace Bud.BuildDefinition {
  public static class BuildDefinitionSettings {
    public static Settings DefaultBuildDefinitionProject(string budDir) {
      return GlobalBuild.New(budDir)
                        .Project(BuildDefinitionPlugin.BuildDefinitionProjectId, budDir, new BuildDefinitionPlugin());
    }
  }
}