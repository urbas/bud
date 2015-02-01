using Bud;
using Bud.CSharp;
using Bud.Projects;
using Bud.Publishing;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings
      .Version("0.0.2")
      .Project("bud", Path.Combine(baseDir, "bud"), Cs.Exe(
        Cs.Dependency("Bud.Core")
      ))
      .Project("Bud.Core", Path.Combine(baseDir, "Core"), Cs.Dll(
        Cs.Dependency("Microsoft.Bcl.Immutable"),
        Cs.Dependency("Newtonsoft.Json"),
        Cs.Dependency("NuGet.Core")
      ), Cs.Test(
        Cs.Dependency("NUnit")
      ))
      .Project("Bud.Test", Path.Combine(baseDir, "Test"), Cs.Dll(
        Cs.Dependency("Bud.Core"),
        Cs.Dependency("NUnit")
      ))
      .Project("Bud.SystemTests", Path.Combine(baseDir, "SystemTests"), Cs.Test(
        Cs.Dependency("Bud.Test")
      ));
  }
}