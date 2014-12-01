using System;
using System.Collections.Immutable;
using System.IO;

namespace Bud.Plugins {

  public static class ProjectPlugin {

    public static readonly ConfigKey<ImmutableHashSet<SettingKey>> ListOfProjects = new ConfigKey<ImmutableHashSet<SettingKey>>("ListOfProjects");
    public static readonly ConfigKey<string> BaseDir = new ConfigKey<string>("BaseDir");
    public static readonly ConfigKey<string> BudDir = new ConfigKey<string>("BudDir");
    public static readonly ConfigKey<string> OutputDir = new ConfigKey<string>("OutputDir");
    public static readonly ConfigKey<string> BuildConfigCacheDir = new ConfigKey<string>("BuildConfigCacheDir");

    public static ScopedSettings Project(string id, string baseDir) {
      var project = Bud.Plugins.Project.New(id);
      return Settings.Start
        .AddProjectSupport()
        .Modify(ListOfProjects, listOfProjects => listOfProjects.Add(project))
        .Initialize(BaseDir.In(project), baseDir)
        .Initialize(BudDir.In(project), b => GetBudDir(b, project))
        .Initialize(OutputDir.In(project), b => GetOutputDir(b, project))
        .Initialize(BuildConfigCacheDir.In(project), b => GetBuildConfigCacheDir(b, project))
        .Initialize(BuildPlugin.Clean.In(project), b => CleanProjectTask(b, project))
        .AddDependencies(BuildPlugin.Clean, BuildPlugin.Clean.In(project))
        .ScopedTo(project);
    }

    public static Settings AddProjectSupport(this Settings existingSettings) {
      return existingSettings
        .AddBuildSupport()
        .EnsureInitialized(ListOfProjects, ImmutableHashSet.Create<SettingKey>());
    }

    private static Unit CleanProjectTask(BuildConfiguration buildConfiguration, ISettingKey project) {
      var outputDir = buildConfiguration.GetOutputDir(project);
      Directory.Delete(outputDir, true);
      return Unit.Instance;
    }

    public static string GetBaseDir(this BuildConfiguration buildConfiguration, ISettingKey project) {
      return buildConfiguration.Evaluate(BaseDir.In(project));
    }

    public static string GetBudDir(this BuildConfiguration buildConfiguration, ISettingKey project) {
      return Path.Combine(GetBaseDir(buildConfiguration, project), ".bud");
    }

    /// <returns>The directory where build output (such as compiled assemblies) are stored.</returns>
    /// <param name="projectBaseDir">The root directory of the project being built.</param>
    public static string GetOutputDir(this BuildConfiguration buildConfiguration, ISettingKey project) {
      return Path.Combine(GetBudDir(buildConfiguration, project), "output");
    }

    /// <returns>The directory where data gathered during build configuration is stored (e.g.: downloaded dependencies).</returns>
    /// <param name="projectBaseDir">The root directory of the project being built.</param>
    public static string GetBuildConfigCacheDir(this BuildConfiguration buildConfiguration, ISettingKey project) {
      return Path.Combine(GetBudDir(buildConfiguration, project), "buildConfigCache");
    }
  }
}

