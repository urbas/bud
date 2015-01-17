using Bud;
using Bud.Plugins.CSharp;
using System.IO;

public class Build : IBuild {
  public Settings SetUp(Settings settings, string baseDir) {
    return settings
      .ExeProject("bud", Path.Combine(baseDir, "bud"),
        CSharp.Dependency("Bud.Core")
      )
      .DllProject("Bud.Core", Path.Combine(baseDir, "Core"),
        CSharp.Dependency("Microsoft.Bcl.Immutable"),
        CSharp.Dependency("Newtonsoft.Json"),
        CSharp.Dependency("NuGet.Core"),
        CSharp.Dependency("NUnit", "2.6.4", target: "test")
      )
      .DllProject("Bud.Test", Path.Combine(baseDir, "Test"),
        CSharp.Dependency("Bud.Core"),
        CSharp.Dependency("NUnit", "2.6.4")
      )
      .DllProject("Bud.SystemTests", Path.Combine(baseDir, "SystemTests"),
        CSharp.Dependency("Bud.Test")
      );
  }
}