using System.Threading.Tasks;
using Bud.Commander;

namespace Bud.BuildDefinition {
  public static class BuildDefinitionConfig {
    public static string GetBuildConfigSourceFile(this IContext context, Key buildLoadingProject) {
      return context.Evaluate(buildLoadingProject / BuildDefinitionKeys.BuildConfigSourceFile);
    }

    public static string GetDirOfProjectToBeBuilt(this IContext context, Key buildLoadingProject) {
      return context.Evaluate(buildLoadingProject / BuildDefinitionKeys.DirOfProjectToBeBuilt);
    }

    public static async Task<IBuildCommander> CreateBuildCommander(this IContext context, Key buildLoadingProject) {
      return await context.Evaluate(buildLoadingProject / BuildDefinitionKeys.CreateBuildCommander);
    }
  }
}