using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;

namespace Bud.Plugins.BuildLoading {
  public static class BuildLoadingKyes {
    public static readonly ConfigKey<string> ProjectToBeBuiltDir = new ConfigKey<string>("ProjectToBeBuiltDir");
    public static readonly TaskKey<Settings> LoadBuildSettings = new TaskKey<Settings>("LoadBuildSettings");
  }
}

