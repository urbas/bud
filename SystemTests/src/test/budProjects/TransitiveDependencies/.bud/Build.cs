using Bud;
using Bud.Plugins.CSharp;
using System.IO;
using System;

public class Build : IBuild {
  public Settings SetUp(Settings settings, string baseDir) {
    return settings
      .DllProject("CommonProject", Path.Combine(baseDir, "CommonProject"))
      .DllProject("A", Path.Combine(baseDir, "A"),
        CSharp.Dependency("Urbas.Example.Foo"),
        CSharp.Dependency("CommonProject")
      )
      .ExeProject("B", Path.Combine(baseDir, "B"), CSharp.Dependency("A"));
  }
}
