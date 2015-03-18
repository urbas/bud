using System;
using System.IO;
using System.Threading.Tasks;
using Bud.Util;

namespace Bud.Build {
  public static class BuildDirs {
    public const string OutputDirName = "output";
    public const string BuildConfigCacheDirName = "buildConfigCache";
    public const string PersistentBuildConfigDirName = "persistentBuildConfig";

    public static Setup Init(string baseDir) {
      return settings => settings
        .Do(BuildDirsKeys.Clean.Init(TaskUtils.NoOpTask),
            BuildDirsKeys.BaseDir.Init(baseDir),
            BuildDirsKeys.BudDir.Init(GetDefaultBudDir),
            BuildDirsKeys.OutputDir.Init(GetDefaultOutputDir),
            BuildDirsKeys.BuildConfigCacheDir.Init(GetDefaultBuildConfigCacheDir),
            BuildDirsKeys.PersistentBuildConfigDir.Init(GetDefaultPersistentBuildConfigDir),
            BuildDirsKeys.Clean.Modify(CleanBuildDirsImpl))
        .Globally(BuildDirsKeys.Clean.Init(TaskUtils.NoOpTask),
                  BuildDirsKeys.Clean.DependsOn(settings.Scope / BuildDirsKeys.Clean));
    }

    public static string GetBaseDir(this IConfig buildConfiguration) {
      return buildConfiguration.GetBaseDir(Key.Root);
    }

    public static string GetBaseDir(this IConfig buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(project / BuildDirsKeys.BaseDir);
    }

    public static string GetBudDir(this IConfig buildConfiguration) {
      return buildConfiguration.GetBudDir(Key.Root);
    }

    public static string GetBudDir(this IConfig buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(project / BuildDirsKeys.BudDir);
    }

    public static string GetDefaultBudDir(this IConfig ctxt, Key key) {
      return Path.Combine(ctxt.GetBaseDir(key), BudPaths.BudDirName);
    }

    /// <returns>The directory where build output (such as compiled assemblies) are stored.</returns>
    public static string GetOutputDir(this IConfig buildConfiguration, Key key) {
      return buildConfiguration.Evaluate(key / BuildDirsKeys.OutputDir);
    }

    public static string GetDefaultOutputDir(this IConfig ctxt, Key key) {
      return Path.Combine(ctxt.GetBudDir(key), OutputDirName);
    }

    /// <returns>
    ///   The directory where transient data gathered during build configuration is stored (e.g.: downloaded
    ///   dependencies).
    /// </returns>
    public static string GetBuildConfigCacheDir(this IConfig buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(project / BuildDirsKeys.BuildConfigCacheDir);
    }

    public static string GetDefaultBuildConfigCacheDir(this IConfig ctxt, Key key) {
      return Path.Combine(ctxt.GetBudDir(key), BuildConfigCacheDirName);
    }

    /// <returns>
    ///   The directory where persistent data gathered during build configuration is stored (e.g.: locked version of downloaded
    ///   dependencies).
    ///   This directoy should be committed to the VCS.
    /// </returns>
    public static string GetPersistentBuildConfigDir(this IConfig buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(project / BuildDirsKeys.PersistentBuildConfigDir);
    }

    public static string GetPersistentBuildConfigDir(this IConfig buildConfiguration) {
      return buildConfiguration.GetPersistentBuildConfigDir(Key.Root);
    }

    public static string GetDefaultPersistentBuildConfigDir(this IConfig ctxt, Key key) {
      return Path.Combine(ctxt.GetBudDir(key), PersistentBuildConfigDirName);
    }

    private static async Task CleanBuildDirsImpl(IContext context, Func<Task> oldCleanTask, Key project) {
      await oldCleanTask();
      var dir = context.GetOutputDir(project);
      if (Directory.Exists(dir)) {
        Directory.Delete(dir, true);
      }
    }
  }
}