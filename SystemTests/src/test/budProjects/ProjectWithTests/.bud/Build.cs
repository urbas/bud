using Bud;
using Bud.Plugins.CSharp;
using System.IO;
using System;

public class Build : IBuild {
  public Settings SetUp(Settings settings, string baseDir) {
    return settings
      .DllProject("A", Path.Combine(baseDir, "A"));
  }
}
