using System.Collections.Immutable;

namespace Bud.Build {
  public static class BuildKeys {
    public static readonly Key Main = Key.Define("main");
    public static readonly TaskKey Test = Key.Define("test");
    public static readonly TaskKey Build = Key.Define("build");
    public static readonly ConfigKey<ImmutableList<Key>> BuildTargets = Key.Define("buildTargets");
  }
}