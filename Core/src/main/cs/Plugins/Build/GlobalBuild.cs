namespace Bud.Plugins.Build {
  public static class GlobalBuild {
    public static Settings New(string globalBuildDir = ".") {
      return Settings.Create(GlobalBuildPlugin.Init((globalBuildDir)));
    }
  }
}