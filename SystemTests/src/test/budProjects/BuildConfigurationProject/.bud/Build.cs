using Bud;
using Bud.Plugins.CSharp;
using Bud.Plugins.Projects;
using System.IO;

public class Build : IBuild {
  public Settings SetUp(Settings settings, string baseDir) {
    return settings
      .ExeProject("Foo", Path.Combine(baseDir, "Foo"))
      .ExeProject("Bar", Path.Combine(baseDir, "Bar"), CSharp.Dependency("Foo"));
  }
}