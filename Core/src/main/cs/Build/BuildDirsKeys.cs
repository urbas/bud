namespace Bud.Build {
  public static class BuildDirsKeys {
    public static readonly ConfigKey<string> BaseDir = Key.Define("baseDir");
    public static readonly ConfigKey<string> BudDir = Key.Define("budDir");
    public static readonly ConfigKey<string> OutputDir = Key.Define("outputDir");
    public static readonly ConfigKey<string> BuildConfigCacheDir = Key.Define("buildConfigCacheDir");
    public static readonly ConfigKey<string> PersistentBuildConfigDir = Key.Define("persistentBuildConfigDir");
    public static readonly TaskKey Clean = Key.Define("clean");
  }
}