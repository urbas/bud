using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;

namespace Bud.Plugins {

  public class Project {
    public readonly string Id;
    public readonly string BaseDir;

    public Project(string id, string baseDir) {
      this.BaseDir = baseDir;
      this.Id = id;
    }
  }

  public static class ProjectPlugin {

    public static readonly ConfigKey<ImmutableHashSet<Project>> ListOfProjects = new ConfigKey<ImmutableHashSet<Project>>();

    public static void Clean(BuildConfiguration buildConfiguration) {
      var listOfProjects = buildConfiguration.Evaluate(ListOfProjects);
      foreach (var project in listOfProjects) {
        Directory.Delete(BudPaths.GetOutputDirectory(project.BaseDir), true);
      }
    }

    public static Settings Project(string id, string baseDir) {
      return InitializePlugin().Modify(ListOfProjects).ByMapping(listOfProjects => listOfProjects.Add(new Project(id, baseDir)));
    }

    public static Settings InitializePlugin() {
      return Settings.Start.EnsureInitialized(ListOfProjects).OrInitializeWith(ImmutableHashSet.Create<Project>());
    }
  }

}

