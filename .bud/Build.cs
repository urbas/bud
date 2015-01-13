using Bud;
using Bud.Plugins.CSharp;
using System.IO;

public class Build : IBuild {
  public Settings SetUp(Settings settings, string baseDir) {
    return settings
      .DllProject("Bud.Core", Path.Combine(baseDir, "Core"),
        CSharp.Dependency("Microsoft.Bcl.Immutable"),
        CSharp.Dependency("Newtonsoft.Json"),
        CSharp.Dependency("Nuget.Core")
      );
  }
}