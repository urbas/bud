using Bud;
using Bud.CSharp;
using Bud.Projects;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    var projectA = new Project("A", Cs.Dll());

    return settings.Add(projectA)
                   .Project("B", Path.Combine(baseDir, "B"), Cs.Exe(Cs.Dependency("A")));
  }
}