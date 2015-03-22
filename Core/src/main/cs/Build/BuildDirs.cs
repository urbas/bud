namespace Bud.Build {
  public static class BuildDirs {
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

    /// <returns>The directory where build output (such as compiled assemblies) are stored.</returns>
    public static string GetOutputDir(this IConfig buildConfiguration, Key key) {
      return buildConfiguration.Evaluate(key / BuildDirsKeys.OutputDir);
    }

    /// <returns>
    ///   The directory where transient data gathered during build configuration is stored (e.g.: downloaded
    ///   dependencies).
    /// </returns>
    public static string GetBuildConfigCacheDir(this IConfig buildConfiguration, Key project) {
      return buildConfiguration.Evaluate(project / BuildDirsKeys.BuildConfigCacheDir);
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
  }
}