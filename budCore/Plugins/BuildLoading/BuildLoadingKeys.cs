using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;

namespace Bud.Plugins.BuildLoading {
  public static class BuildLoadingKeys {
    public static readonly ConfigKey<string> DirOfProjectToBeBuilt = new ConfigKey<string>("DirOfProjectToBeBuilt");
    public static readonly TaskKey<Settings> LoadBuildSettings = new TaskKey<Settings>("LoadBuildSettings");
  }
}

