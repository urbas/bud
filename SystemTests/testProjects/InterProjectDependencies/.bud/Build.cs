using Bud;
using Bud.Plugins.Projects;
using Bud.Plugins.CSharp;
using System.IO;
using System;

public class Build : IBuild {
  public Settings SetUp(Settings settings, string baseDir) {
    return settings
      .LibraryProject("A", Path.Combine(baseDir, "A"))
      .CSharpProject("B", Path.Combine(baseDir, "B"), Project.Dependency("A"));
  }
}
