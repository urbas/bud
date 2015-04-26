using Bud;
using Bud.CSharp;
using Bud.Projects;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings.Project("A", baseDir, Cs.Test(
        Cs.Dependency("Urbas.Example.Foo")
    ));
  }
}