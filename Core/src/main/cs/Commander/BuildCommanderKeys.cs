namespace Bud.Commander {
  public static class BuildCommanderKeys {
    public static readonly ConfigKey<string> BuildConfigSourceFile = Key.Define("buildConfigSourceFile");
    public static readonly ConfigKey<string> DirOfProjectToBeBuilt = Key.Define("dirOfProjectToBeBuilt");
    public static readonly TaskKey<IBuildCommander> CreateBuildCommander = Key.Define("createBuildCommander");
  }
}