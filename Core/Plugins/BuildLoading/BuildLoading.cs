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
      var buildProjectDir = Path.Combine(path, ".bud");
      var buildProjectId = "BuildDefinition";
      var buildProject = GlobalBuild.New(buildProjectDir).BuildProject(buildProjectId, buildProjectDir, path);
      var evaluationContext = EvaluationContext.FromSettings(buildProject);
      return await evaluationContext.CreateBuildCommander(Project.ProjectScope(buildProjectId));
    }

    public static IBuildCommander Load(string path) {
      return CreateBuildCommander(path).Result;
    }

  }
}

