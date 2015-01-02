using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Bud.Plugins.Build;
using Bud.Plugins.BuildLoading;
using System.Threading.Tasks;
using System.IO;
using Bud.Plugins.Projects;

namespace Bud.Commander {
  public static class BuildCommander {

    public static IBuildCommander Load(string path) {
      var buildProjectDir = Path.Combine(path, BuildDirs.BudDirName);
      var buildProjectId = "BuildDefinition";
      var buildProject = GlobalBuild.New(buildProjectDir).BuildProject(buildProjectId, buildProjectDir, path);
      var evaluationContext = EvaluationContext.FromSettings(buildProject);
      var buildCommanderTask = evaluationContext.CreateBuildCommander(Project.ProjectScope(buildProjectId));
      buildCommanderTask.Wait();
      return buildCommanderTask.Result;
    }

    public static object Evaluate(this IBuildCommander budCommander, Key scope) {
      return budCommander.Evaluate(scope.ToString());
    }
  }
}
