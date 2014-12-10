using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;
using System.Threading.Tasks;
using Bud.Commander;

namespace Bud.Plugins.BuildLoading {
  public static class BuildLoading {
    public static Settings Project(string projectId, string budDir, string dirOfProjectToBeBuilt) {
      return CSharp.CSharp.Project(projectId, budDir)
        .Add(new BuildLoadingPlugin(dirOfProjectToBeBuilt));
    }

    public static string GetBuildConfigSourceFile(this EvaluationContext context, Scope buildLoadingProject) {
      return context.Evaluate(BuildLoadingKeys.BuildConfigSourceFile.In(buildLoadingProject));
    }

    public async static Task<IBuildCommander> CreateBuildCommander(this EvaluationContext context, Scope buildLoadingProject) {
      return await context.Evaluate(BuildLoadingKeys.CreateBuildCommander.In(buildLoadingProject));
    }

    public static string GetDirOfProjectToBeBuilt(this EvaluationContext context, Scope buildLoadingProject) {
      return context.Evaluate(BuildLoadingKeys.DirOfProjectToBeBuilt.In(buildLoadingProject));
    }

    public async static Task<IBuildCommander> CreateBuildCommander(string path) {
      var buildProject = BuildLoading.Project("BuildDefinition", Path.Combine(path, ".bud"), path);
      var evaluationContext = EvaluationContext.FromSettings(buildProject);
      return await evaluationContext.CreateBuildCommander(buildProject.CurrentScope);
    }

    public static IBuildCommander Load(string path) {
      return CreateBuildCommander(path).Result;
    }

  }
}

