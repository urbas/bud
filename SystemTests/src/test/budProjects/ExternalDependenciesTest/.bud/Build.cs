using Bud;
using Bud.CSharp;
using Bud.Projects;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    var projectFoo = new Project("Foo", baseDir, Cs.Exe(
        Cs.Dependency("Urbas.Example.Foo", "1.0.0")
    ));
    return settings.Add(projectFoo);
  }
}