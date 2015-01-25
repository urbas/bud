using System;
using System.IO;
using System.Threading.Tasks;
using Bud.Util;

namespace Bud.Build {
  public static class BuildDirs {
    public const string BudDirName = ".bud";
    public const string OutputDirName = "output";
    public const string BuildConfigCacheDirName = "buildConfigCache";
    public const string PersistentBuildConfigDirName = "persistentBuildConfig";

    public static Setup Init(string baseDir) {
      return settings => settings
        .Do(
          BuildDirsKeys.Clean.Init(TaskUtils.NoOpTask),
          BuildDirsKeys.BaseDir.Init(baseDir),
          BuildDirsKeys.BudDir.Init(GetDefaultBudDir),
          BuildDirsKeys.OutputDir.Init(GetDefaultOutputDir),
          BuildDirsKeys.BuildConfigCacheDir.Init(GetDefaultBuildConfigCacheDir),
          BuildDirsKeys.PersistentBuildConfigDir.Init(GetDefaultPersistentBuildConfigDir),
          BuildDirsKeys.Clean.Modify(CleanBuildDirsTask)
        )
        .In(Key.Global,
            BuildDirsKeys.Clean.Init(TaskUtils.NoOpTask),
            BuildDirsKeys.Clean.DependsOn(BuildDirsKeys.Clean.In(settings.Scope))
        );
    }

    public static string GetBaseDir(this IConfig buildConfiguration) {
      return buildConfiguration.GetBaseDir(Key.Global);
    }

    public static string GetBaseDir(this IConfig buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BaseDir.In(project));
    }

    public static string GetBudDir(this IConfig buildConfiguration) {
      return buildConfiguration.GetBudDir(Key.Global);
    }

    public static string GetBudDir(this IConfig buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BudDir.In(project));
    }

    public static string GetDefaultBudDir(this IConfig ctxt, Key key) {
      return Path.Combine(ctxt.GetBaseDir(key), BudDirName);
    }

    /// <returns>The directory where build output (such as compiled assemblies) are stored.</returns>
    public static string GetOutputDir(this IConfig buildConfiguration, Key key) {
      return buildConfiguration.Evaluate(BuildDirsKeys.OutputDir.In(key));
    }

    public static string GetDefaultOutputDir(this IConfig ctxt, Key key) {
      return Path.Combine(ctxt.GetBudDir(key), OutputDirName);
    }

    /// <returns>
    ///   The directory where transient data gathered during build configuration is stored (e.g.: downloaded
    ///   dependencies).
    /// </returns>
    public static string GetBuildConfigCacheDir(this IConfig buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BuildConfigCacheDir.In(project));
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
      return buildConfiguration.Evaluate(BuildDirsKeys.PersistentBuildConfigDir.In(project));
    }

    public static string GetPersistentBuildConfigDir(this IConfig buildConfiguration) {
      return buildConfiguration.GetPersistentBuildConfigDir(Key.Global);
    }

    public static string GetDefaultPersistentBuildConfigDir(this IConfig ctxt, Key key) {
      return Path.Combine(ctxt.GetBudDir(key), PersistentBuildConfigDirName);
    }

    public static string CreatePersistentBuildConfigDir(this IContext context) {
      var persistentConfigDir = context.GetPersistentBuildConfigDir();
      Directory.CreateDirectory(persistentConfigDir);
      return persistentConfigDir;
    }

    private static async Task CleanBuildDirsTask(IContext context, Func<Task> oldCleanTask, Key project) {
      await oldCleanTask();
      var dir = context.GetOutputDir(project);
      if (Directory.Exists(dir)) {
        Directory.Delete(dir, true);
      }
    }
  }
}