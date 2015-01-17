using Bud;
using Bud.Plugins.CSharp;
using System.IO;
using System;

public class Build : IBuild {
  public Settings SetUp(Settings settings, string baseDir) {
    return settings
      .ExeProject("Foo", baseDir, CSharp.Dependency("Urbas.Example.Foo", "1.0.0"));
  }
}