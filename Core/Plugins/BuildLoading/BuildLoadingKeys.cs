using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;
using Bud.Commander;

namespace Bud.Plugins.BuildLoading {
  public static class BuildLoadingKeys {
    public static readonly ConfigKey<string> BuildConfigSourceFile = new ConfigKey<string>("BuildConfigSourceFile");
    public static readonly ConfigKey<string> DirOfProjectToBeBuilt = new ConfigKey<string>("DirOfProjectToBeBuilt");
    public static readonly TaskKey<IBuildCommander> CreateBuildCommander = new TaskKey<IBuildCommander>("CreateBuildCommander");
  }
}

