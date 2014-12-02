using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace Bud.Plugins.Projects {

  public static class ProjectPlugin {

    public static ScopedSettings AddProject(this Settings existingSettings, string id, string baseDir) {
      var project = Project.Key(id);
      return existingSettings
        .AddProjectSupport()
        .Modify(ProjectKeys.ListOfProjects, listOfProjects => listOfProjects.Add(project))
        .AddDependencies(BuildPlugin.Clean, BuildPlugin.Clean.In(project))
        .ScopedTo(project)
        .Initialize(ProjectKeys.BaseDir, baseDir)
        .Initialize(ProjectKeys.BudDir, Project.GetDefaultBudDir)
        .Initialize(ProjectKeys.OutputDir, Project.GetDefaultOutputDir)
        .Initialize(ProjectKeys.BuildConfigCacheDir, Project.GetDefaultBuildConfigCacheDir)
        .Initialize(BuildPlugin.Clean, CleanProjectTask);
    }

    private static Settings AddProjectSupport(this Settings existingSettings) {
      return existingSettings
        .AddBuildSupport()
        .EnsureInitialized(ProjectKeys.ListOfProjects, ImmutableHashSet.Create<Scope>());
    }

    private static Unit CleanProjectTask(EvaluationContext context, Scope project) {
      var outputDir = context.GetOutputDir(project);
      Directory.Delete(outputDir, true);
      return Unit.Instance;
    }
  }
}

