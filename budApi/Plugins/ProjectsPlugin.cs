using System;
using System.Collections.Immutable;
using Bud.SettingsConstruction.Ops;
using System.IO;

namespace Bud.Plugins {

  public static class ProjectPlugin {

    public static readonly ConfigKey<ImmutableHashSet<Project>> ListOfProjects = new ConfigKey<ImmutableHashSet<Project>>();

    public static void Clean(BuildConfiguration buildConfiguration) {
      var listOfProjects = buildConfiguration.Evaluate(ListOfProjects);
      foreach (var project in listOfProjects) {
        Directory.Delete(BudPaths.GetOutputDirectory(project.BaseDir), true);
      }
    }

    public static Settings Project(string id, string baseDir) {
      return InitializePlugin().Add(ConfigModification.Create(ListOfProjects, listOfProjects => listOfProjects.Add(new Project(id, baseDir))));
    }

    public static Settings InitializePlugin() {
      return Settings.Start.Add(SettingEnsureInitialization.Create(ListOfProjects, ImmutableHashSet.Create<Project>()));
    }
  }
}

