namespace Bud.Commander {
  public static class BuildCommanderKeys {
    public static readonly ConfigKey<string> BuildConfigSourceFile = ConfigKey<string>.Define("buildConfigSourceFile");
    public static readonly ConfigKey<string> DirOfProjectToBeBuilt = ConfigKey<string>.Define("dirOfProjectToBeBuilt");
    public static readonly TaskKey<IBuildCommander> CreateBuildCommander = TaskKey<IBuildCommander>.Define("createBuildCommander");
  }
}