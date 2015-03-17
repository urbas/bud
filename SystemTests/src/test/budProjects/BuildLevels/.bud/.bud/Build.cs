using Bud;
using Bud.CSharp;
using Bud.Projects;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings.BuildDefinition(
      Cs.Dependency("Bud.Examples.HelloWorldPlugin")
    );
  }
}