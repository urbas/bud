using Bud.Commander;

namespace Bud.Plugins.BuildLoading {
  public static class BuildLoadingKeys {
    public static readonly ConfigKey<string> BuildConfigSourceFile = new ConfigKey<string>("buildConfigSourceFile");
    public static readonly ConfigKey<string> DirOfProjectToBeBuilt = new ConfigKey<string>("dirOfProjectToBeBuilt");
    public static readonly TaskKey<IBuildCommander> CreateBuildCommander = new TaskKey<IBuildCommander>("createBuildCommander");
  }
}