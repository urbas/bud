using System;

namespace Bud.Plugins.Build {
  public static class BuildKeys {
    public static readonly TaskKey<Unit> Test = new TaskKey<Unit>("test");
    public static readonly TaskKey<Unit> Build = new TaskKey<Unit>("build");
    public static readonly TaskKey<Unit> Clean = new TaskKey<Unit>("clean");
  }
}

