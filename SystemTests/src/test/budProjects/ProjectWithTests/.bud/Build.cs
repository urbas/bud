using Bud;
using Bud.CSharp;
using Bud.Projects;
using System.IO;
using System;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings
      .Project("A", Path.Combine(baseDir, "A"), Cs.Dll(), Cs.Test());
  }
}
