using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;

namespace Bud.Plugins.Build {

  public static class BuildDirs {
    public const string BudDirName = ".bud";
    public const string OutputDirName = "output";
    public const string BuildConfigCacheDirName = "buildConfigCache";
    public const string PersistentBuildConfigDirName = "persistentBuildConfig";

    public static string GetBaseDir(this IConfiguration buildConfiguration) {
      return buildConfiguration.GetBaseDir(Key.Global);
    }

    public static string GetBaseDir(this IConfiguration buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BaseDir.In(project));
    }

    public static string GetBudDir(this IConfiguration buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BudDir.In(project));
    }

    public static string GetDefaultBudDir(this Configuration ctxt, Key key) {
      return Path.Combine(ctxt.GetBaseDir(key), BudDirName);
    }

    /// <returns>The directory where build output (such as compiled assemblies) are stored.</returns>
    public static string GetOutputDir(this IConfiguration buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.OutputDir.In(project));
    }

    public static string GetDefaultOutputDir(this IConfiguration ctxt, Key project) {
      return Path.Combine(ctxt.GetBudDir(project), OutputDirName);
    }

    /// <returns>The directory where transient data gathered during build configuration is stored (e.g.: downloaded dependencies).</returns>
    public static string GetBuildConfigCacheDir(this IConfiguration buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BuildConfigCacheDir.In(project));
    }

    public static string GetDefaultBuildConfigCacheDir(this IConfiguration ctxt, Key key) {
      return Path.Combine(ctxt.GetBudDir(key), BuildConfigCacheDirName);
    }

    /// <returns>
    /// The directory where persistent data gathered during build configuration is stored (e.g.: locked version of downloaded dependencies).
    /// 
    /// This directoy should be committed to the VCS.
    /// </returns>
    public static string GetPersistentBuildConfigDir(this IConfiguration buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.PersistentBuildConfigDir.In(project));
    }

    public static string GetDefaultPersistentBuildConfigDir(this IConfiguration ctxt, Key key) {
      return Path.Combine(ctxt.GetBudDir(key), PersistentBuildConfigDirName);
    }
  }

}

