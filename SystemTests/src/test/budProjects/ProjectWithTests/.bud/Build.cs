using Bud;
using Bud.CSharp;
using Bud.Projects;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    var projectA = new Project("A", Path.Combine(baseDir, "A"), Cs.Dll(), Cs.Test());
    return settings.Add(projectA);
  }
}
