using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace Bud.Plugins.Projects {

  public static class ProjectPlugin {
    public static Settings AddProject(this Settings existingSettings, string id, string baseDir) {
      var project = existingSettings.CreateProjectScope(id);
      return existingSettings
        .AddProjectSupport()
        .Modify(ProjectKeys.ListOfProjects, listOfProjects => listOfProjects.Add(project))
        .AddDependencies(BuildPlugin.Clean, BuildPlugin.Clean.In(project))
        .ScopedTo(project)
        .Init(ProjectKeys.BaseDir.In(project), baseDir)
        .Init(ProjectKeys.BudDir.In(project), ctxt => Project.GetDefaultBudDir(ctxt, project))
        .Init(ProjectKeys.OutputDir.In(project), ctxt => Project.GetDefaultOutputDir(ctxt, project))
        .Init(ProjectKeys.BuildConfigCacheDir.In(project), ctxt => Project.GetDefaultBuildConfigCacheDir(ctxt, project))
        .Init(BuildPlugin.Clean.In(project), ctxt => CleanProjectTask(ctxt, project));
    }

    private static Settings AddProjectSupport(this Settings existingSettings) {
      return existingSettings
        .AddBuildSupport()
        .InitOrKeep(ProjectKeys.ListOfProjects, ImmutableHashSet.Create<Scope>());
    }

    private static Unit CleanProjectTask(EvaluationContext context, Scope project) {
      var outputDir = context.GetOutputDir(project);
      Directory.Delete(outputDir, true);
      return Unit.Instance;
    }
  }
}

