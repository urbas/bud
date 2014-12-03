using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;
using System.Threading.Tasks;

namespace Bud.Plugins.BuildLoading {
  public static class BuildLoading {
    public static Settings Project(string projectId, string baseDir, string dirOfProjectToBeBuilt) {
      return CSharp.CSharp.Project(projectId, baseDir).Add(new BuildLoadingPlugin(dirOfProjectToBeBuilt));
    }

    public async static Task<Settings> LoadBuildSettings(this EvaluationContext context, Scope buildLoadingProject) {
      return await context.Evaluate(BuildLoadingKyes.LoadBuildSettings);
    }

    public async static Task<Settings> LoadBuildSettings(string path) {
      var buildProject = BuildLoading.Project("BuildDefinition", Path.Combine(path, ".bud"), path);
      var evaluationContext = EvaluationContext.FromSettings(buildProject);
      return await evaluationContext.LoadBuildSettings(buildProject.CurrentScope);
    }

    public static EvaluationContext Load(string path) {
      // Does the .bud/bakedBuild/Build.dll file exist?
      //  - load it and be done with it :)
      return EvaluationContext.FromSettings(LoadBuildSettings(path).Result);
    }

  }
}

