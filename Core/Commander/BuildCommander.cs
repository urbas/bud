using System.IO;
using Bud.Plugins.Build;
using Bud.Plugins.BuildLoading;
using Bud.Plugins.Projects;

namespace Bud.Commander {
  public static class BuildCommander {
    public static IBuildCommander Load(string path) {
      var buildProjectDir = Path.Combine(path, BuildDirs.BudDirName);
      const string buildProjectId = "BuildDefinition";
      var buildProject = GlobalBuild.New(buildProjectDir).BuildProject(buildProjectId, buildProjectDir, path);
      var evaluationContext = Context.FromSettings(buildProject);
      var buildCommanderTask = evaluationContext.CreateBuildCommander(Project.ProjectKey(buildProjectId));
      buildCommanderTask.Wait();
      return buildCommanderTask.Result;
    }
  }
}