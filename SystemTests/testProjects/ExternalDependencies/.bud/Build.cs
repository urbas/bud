using Bud;
using Bud.Plugins.CSharp;
using Bud.Plugins.NuGet;
using System.IO;
using System;

public class Build : IBuild {
  public Settings GetSettings(string baseDir) {
    return CSharp.Project("Foo", baseDir)
      .NuGet("Microsoft.Bcl.Immutable", "1.0.34");
  }
}