using Bud;
using Bud.CSharp;
using Bud.Projects;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings
      .Project("A", baseDir, Cs.Dll(Cs.Dependency("Urbas.Example.Bar", "1.0.1")));
  }
}