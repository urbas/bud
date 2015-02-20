using NuGet;

namespace Bud.Build {
  public static class BuildKeys {
    public static readonly Key Main = Key.Define("main");
    public static readonly TaskKey Test = TaskKey.Define("test");
    public static readonly TaskKey Build = TaskKey.Define("build");
    public static readonly ConfigKey<SemanticVersion> Version = ConfigKey<SemanticVersion>.Define("version");
  }
}