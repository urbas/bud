using Bud;
using Bud.Plugins;
using Bud.Plugins.CSharp;

public class BuildWithDependencies : Build {
  public Settings GetSettings(string baseDir) {
    return Project.New("root", baseDir)
      .BuildsCSharp()
      .WithDependency("Foo.Bar", "0.1.3");
  }
}


