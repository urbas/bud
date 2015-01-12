using System.Threading.Tasks;
using Bud.Commander;
using Bud.Plugins.CSharp;

namespace Bud.Plugins.BuildLoading {
  public static class BuildLoading {
    public static Settings BuildProject(this Settings build, string projectId, string budDir, string dirOfProjectToBeBuilt) {
      return build
        .ExeProject(projectId, budDir, new BuildLoadingPlugin(dirOfProjectToBeBuilt));
    }

    public static string GetBuildConfigSourceFile(this IContext context, Key buildLoadingProject) {
      return context.Evaluate(BuildLoadingKeys.BuildConfigSourceFile.In(buildLoadingProject));
    }

    public static async Task<IBuildCommander> CreateBuildCommander(this IContext context, Key buildLoadingProject) {
      return await context.Evaluate(BuildLoadingKeys.CreateBuildCommander.In(buildLoadingProject));
    }

    public static string GetDirOfProjectToBeBuilt(this IContext context, Key buildLoadingProject) {
      return context.Evaluate(BuildLoadingKeys.DirOfProjectToBeBuilt.In(buildLoadingProject));
    }
  }
}