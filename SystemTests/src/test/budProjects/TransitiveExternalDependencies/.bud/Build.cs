using Bud;
using Bud.Plugins.CSharp;
using System.IO;
using System;

public class Build : IBuild {
  public Settings SetUp(Settings settings, string baseDir) {
    return settings
      .DllProject("A", baseDir, CSharp.Dependency("Urbas.Example.Bar", "1.0.1"));
  }
}