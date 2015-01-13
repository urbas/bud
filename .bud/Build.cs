using Bud;
using Bud.Plugins.CSharp;
using System.IO;

public class Build : IBuild {
  public Settings SetUp(Settings settings, string baseDir) {
    return settings
      .DllProject("Bud.Graph", Path.Combine(baseDir, "Graph"),
        CSharp.Dependency("Microsoft.Bcl.Immutable")
      );
  }
}