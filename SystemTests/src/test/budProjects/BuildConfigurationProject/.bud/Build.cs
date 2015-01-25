using Bud;
using Bud.CSharp;
using Bud.Projects;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings
      .Project("Foo", Path.Combine(baseDir, "Foo"), Cs.Exe())
      .Project("Bar", Path.Combine(baseDir, "Bar"), Cs.Exe(
        Cs.Dependency("Foo")
      ));
  }
}