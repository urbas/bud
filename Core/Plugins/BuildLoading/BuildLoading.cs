using System;
using Bud.Plugins;
using Bud.Plugins.CSharp;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;
using System.Threading.Tasks;
using Bud.Commander;
using Bud.Plugins.Build;

namespace Bud.Plugins.BuildLoading {
  public static class BuildLoading {
    public static Settings BuildProject(this Settings build, string projectId, string budDir, string dirOfProjectToBeBuilt) {
      return build
        .CSharpProject(projectId, budDir, new BuildLoadingPlugin(dirOfProjectToBeBuilt));
    }

    public static string GetBuildConfigSourceFile(this EvaluationContext context, Key buildLoadingProject) {
      return context.Evaluate(BuildLoadingKeys.BuildConfigSourceFile.In(buildLoadingProject));
    }

    public async static Task<IBuildCommander> CreateBuildCommander(this EvaluationContext context, Key buildLoadingProject) {
      return await context.Evaluate(BuildLoadingKeys.CreateBuildCommander.In(buildLoadingProject));
    }

    public static string GetDirOfProjectToBeBuilt(this EvaluationContext context, Key buildLoadingProject) {
      return context.Evaluate(BuildLoadingKeys.DirOfProjectToBeBuilt.In(buildLoadingProject));
    }
  }
}

