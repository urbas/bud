using System.IO;
using System.Collections.Immutable;
using Bud.Settings;

namespace Bud.Plugins {

  public class ProjectPlugin {

    public static readonly ConfigKey<ImmutableHashSet<string>> ListOfProjects = new ConfigKey<ImmutableHashSet<string>>();

    public static void Clean(BuildConfiguration buildConfiguration) {
      Directory.Delete(BudPaths.GetOutputDirectory(buildConfiguration.ProjectBaseDir), true);
    }

    public static ImmutableList<Setting> CreateProject(string id, string baseDir) {
      return SettingsUtils
        .Start
        .EnsureInitialized(ListOfProjects).OrInitializeWith(ImmutableHashSet.Create<string>())
        .Modify(ListOfProjects).ByMapping(listOfProjects => listOfProjects.Add(id));
    }
  }

}

