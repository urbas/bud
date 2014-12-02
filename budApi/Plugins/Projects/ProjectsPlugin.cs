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
        .Initialize(ProjectKeys.BaseDir.In(project), baseDir)
        .Initialize(ProjectKeys.BudDir.In(project), ctxt => Project.GetDefaultBudDir(ctxt, project))
        .Initialize(ProjectKeys.OutputDir.In(project), ctxt => Project.GetDefaultOutputDir(ctxt, project))
        .Initialize(ProjectKeys.BuildConfigCacheDir.In(project), ctxt => Project.GetDefaultBuildConfigCacheDir(ctxt, project))
        .Initialize(BuildPlugin.Clean.In(project), ctxt => CleanProjectTask(ctxt, project))
        .ScopedTo(project);
    }

    private static Settings AddProjectSupport(this Settings existingSettings) {
      return existingSettings
        .AddBuildSupport()
        .EnsureInitialized(ProjectKeys.ListOfProjects, ImmutableHashSet.Create<SettingKey>());
    }

    private static Unit CleanProjectTask(EvaluationContext context, ISettingKey project) {
      var outputDir = context.GetOutputDir(project);
      Directory.Delete(outputDir, true);
      return Unit.Instance;
    }
  }
}

