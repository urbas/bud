namespace Bud.Plugins.Build {
  public static class BuildKeys {
    public static readonly Key Main = new Key("main");
    public static readonly TaskKey<Unit> Test = new TaskKey<Unit>("test");
    public static readonly TaskKey<Unit> Build = new TaskKey<Unit>("build");
  }
}