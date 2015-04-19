using Bud;
using Bud.CSharp;
using Bud.Projects;
using Bud.Resources;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings.Add(new Project("Foo", baseDir, Cs.Exe(), Res.Main()));
  }
}
