namespace Bud.Plugins.Build {
  public static class GlobalBuild {
    public static Settings New(string globalBuildDir = ".") {
      return Settings.Empty.Apply(Key.Global, new GlobalBuildPlugin(globalBuildDir));
    }
  }
}