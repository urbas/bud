using Bud;
using Bud.Plugins.CSharp;
using Bud.Plugins.NuGet;
using System.IO;
using System;

public class Build : IBuild {
  public Settings SetUp(Settings settings, string baseDir) {
    return settings
      .CSharpProject("Foo", baseDir, NuGet.Dependency("Urbas.Example.Foo"));
  }
}
