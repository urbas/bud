using System;
using System.Collections.Immutable;
using Bud.SettingsConstruction.Ops;
using System.IO;
using Bud.SettingsConstruction;

namespace Bud.Plugins {

  public static class ProjectPlugin {

    public static readonly ConfigKey<ImmutableHashSet<Project>> ListOfProjects = new ConfigKey<ImmutableHashSet<Project>>("ListOfProjects");
    public static readonly ConfigKey<string> BaseDir = new ConfigKey<string>("BaseDir");

    public static ScopedSettings Project(string id, string baseDir) {
      var project = new Project(id);
      return Settings.Start
        .AddProjectSupport()
        .Modify(ListOfProjects, listOfProjects => listOfProjects.Add(project))
        .Initialize(BaseDir.In(project), baseDir)
        .Modify(BuildPlugin.Clean, CleanTask)
        .ScopedTo(project);
    }

    public static Settings AddProjectSupport(this Settings existingSettings) {
      return existingSettings
        .AddBuildSupport()
        .EnsureInitialized(ListOfProjects, ImmutableHashSet.Create<Project>());
    }

    private static Unit CleanTask(Func<Unit> previousCleanTask, BuildConfiguration buildConfiguration) {
      previousCleanTask();
      var listOfProjects = buildConfiguration.Evaluate(ListOfProjects);
      foreach (var project in listOfProjects) {
        var baseDir = buildConfiguration.Evaluate(BaseDir.In(project));
        Directory.Delete(BudPaths.GetOutputDirectory(baseDir), true);
      }
      return Unit.Instance;
    }
  }
}

