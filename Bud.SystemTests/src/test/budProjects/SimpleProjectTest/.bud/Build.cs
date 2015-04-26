using Bud;
using Bud.CSharp;
using Bud.Projects;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings.Add(new Project("Foo", baseDir, Cs.Exe()));
  }
}
