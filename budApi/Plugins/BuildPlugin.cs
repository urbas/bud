using System.IO;
using System.Collections.Immutable;
using Bud.Settings;

namespace Bud.Plugins {

  public class BuildPlugin {

    public static readonly SettingKey ProjectSettingKey = new SettingKey();

    public static void Clean(BuildConfiguration buildConfiguration) {
      Directory.Delete(BudPaths.GetOutputDirectory(buildConfiguration.ProjectBaseDir), true);
    }

    public static ImmutableList<Setting> AddProject(string baseDir) {
      throw new System.NotImplementedException();
    }

    public static ImmutableList<Setting> In(string pathToProject) {
      return SettingsUtils.Start.InitializeSetting(ProjectSettingKey).WithValue(ImmutableHashSet.Create<string>());
    }
  }

}

