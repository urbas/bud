namespace Bud.Build {
  public static class BuildDirsKeys {
    public static readonly ConfigKey<string> BaseDir = ConfigKey<string>.Define("baseDir");
    public static readonly ConfigKey<string> BudDir = ConfigKey<string>.Define("budDir");
    public static readonly ConfigKey<string> OutputDir = ConfigKey<string>.Define("outputDir");
    public static readonly ConfigKey<string> BuildConfigCacheDir = ConfigKey<string>.Define("buildConfigCacheDir");
    public static readonly ConfigKey<string> PersistentBuildConfigDir = ConfigKey<string>.Define("persistentBuildConfigDir");
    public static readonly TaskKey Clean = TaskKey.Define("clean");
  }
}