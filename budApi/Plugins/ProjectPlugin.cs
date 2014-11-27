using System.IO;
using System.Collections.Immutable;
using Bud.Settings;

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
      Directory.Delete(BudPaths.GetOutputDirectory(buildConfiguration.ProjectBaseDir), true);
    }

    public static ImmutableList<Setting> AddProject(this ImmutableList<Setting> existingSettings, string id, string baseDir) {
      return existingSettings.Modify(ListOfProjects).ByMapping(listOfProjects => listOfProjects.Add(new Project(id, baseDir)));
    }

    public static ImmutableList<Setting> InitializePlugin(this ImmutableList<Setting> existingSettings) {
      return existingSettings.EnsureInitialized(ListOfProjects).OrInitializeWith(ImmutableHashSet.Create<Project>());
    }
  }

}

