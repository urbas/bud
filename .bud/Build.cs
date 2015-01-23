using Bud;
using Bud.Plugins.CSharp;
using Bud.Plugins.Projects;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings
      .Project("bud", Path.Combine(baseDir, "bud"), Cs.Exe(
        Cs.Dependency("Bud.Core")
      ))
      .Project("Bud.Core", Path.Combine(baseDir, "Core"), Cs.Dll(
        Cs.Dependency("Microsoft.Bcl.Immutable"),
        Cs.Dependency("Newtonsoft.Json"),
        Cs.Dependency("NuGet.Core")
      ), Cs.Test(
        Cs.Dependency("NUnit", "2.6.4")
      ))
      .Project("Bud.Test", Path.Combine(baseDir, "Test"), Cs.Dll(
        Cs.Dependency("Bud.Core"),
        Cs.Dependency("NUnit", "2.6.4")
      ))
      .Project("Bud.SystemTests", Path.Combine(baseDir, "SystemTests"), Cs.Test(
        Cs.Dependency("Bud.Test")
      ));
  }
}