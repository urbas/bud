using System;

namespace Bud.Plugins.Build {
  public static class BuildKeys {
    public static readonly TaskKey<Unit> Build = new TaskKey<Unit>("Build");
    public static readonly TaskKey<Unit> Clean = new TaskKey<Unit>("Clean");
  }
}

