using Bud;
using Bud.Plugins.CSharp;
using Bud.Plugins.Projects;
using System.IO;
using System;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings
      .Project("A", baseDir, Cs.Exe(
        TargetFramework.Net35,
        Dependency("Urbas.Example.Foo")
      ));
  }
}