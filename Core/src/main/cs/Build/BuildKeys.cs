﻿using NuGet;

namespace Bud.Build {
  public static class BuildKeys {
    public static readonly Key Main = new Key("main");
    public static readonly TaskKey Test = new TaskKey("test");
    public static readonly TaskKey Build = new TaskKey("build");
    public static readonly ConfigKey<SemanticVersion> Version = new ConfigKey<SemanticVersion>("version");
  }
}