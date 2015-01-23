using Bud;
using Bud.Plugins.CSharp;
using Bud.Plugins.Projects;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings
      .Project("CommonProject", Path.Combine(baseDir, "CommonProject"), Cs.Dll())
      .Project("A", Path.Combine(baseDir, "A"), Cs.Dll(
        Cs.Dependency("Urbas.Example.Foo"),
        Cs.Dependency("CommonProject")
      ))
      .Project("B", Path.Combine(baseDir, "B"), Cs.Exe(
        Cs.Dependency("A")
      ));
  }
}
