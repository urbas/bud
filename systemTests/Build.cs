using Bud;
using Bud.Plugins.CSharp;
using Bud.Plugins.Dependencies;
using Bud.Plugins.Projects;

public class Build : BuildDefinition {
  public Settings GetSettings(string baseDir) {
    return Project.New("root", baseDir)
      .Add(CSharpPlugin.Instance)
      .WithDependency("Foo.Bar", "0.1.3");
  }
}