using Bud;
using Bud.CSharp;
using Bud.Projects;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings
      .Project("A", Path.Combine(baseDir, "A"), Cs.Dll())
      .Project("B", Path.Combine(baseDir, "B"), Cs.Exe(Cs.Dependency("A")));
  }
}