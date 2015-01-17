using System.IO;

namespace Bud.Plugins.Build {
  public static class BuildDirs {
    public const string BudDirName = ".bud";
    public const string OutputDirName = "output";
    public const string BuildConfigCacheDirName = "buildConfigCache";
    public const string PersistentBuildConfigDirName = "persistentBuildConfig";

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
  }
}