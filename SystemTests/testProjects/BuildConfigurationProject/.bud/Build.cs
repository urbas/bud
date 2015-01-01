using Bud;
using Bud.Plugins.CSharp;
using Bud.Plugins.Projects;
using System.IO;

public class Build : IBuild {
  public Settings SetUp(Settings settings, string baseDir) {
    return settings
      .CSharpProject("Foo", Path.Combine(baseDir, "Foo"))
      .CSharpProject("Bar", Path.Combine(baseDir, "Bar"), Project.Dependency("Foo"));
  }
}