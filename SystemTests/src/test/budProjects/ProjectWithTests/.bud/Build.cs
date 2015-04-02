using Bud;
using Bud.CSharp;
using Bud.Projects;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    var projectA = new Project(
      "A",
      Path.Combine(baseDir, "A"),
      Cs.Exe(),
      Cs.Test(
        Cs.Dependency("NUnit")
      )
    );

    return settings.Add(projectA);
  }
}
