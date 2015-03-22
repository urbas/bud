using Bud;
using Bud.CSharp;
using Bud.Projects;
using Bud.Resources;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings
      .AddGlobally(CSharpKeys.TargetFramework.Init(Framework.Net46))
      .Version("0.0.3")
      .Project("bud", Path.Combine(baseDir, "bud"), Cs.Exe(
        Cs.Dependency("Bud.Core"),
        Cs.Dependency("CommandLineParser"),
        CSharpKeys.RootNamespace.Modify("Bud")
      ))
      .Project("Bud.Core", Path.Combine(baseDir, "Core"), Res.Main(), Cs.Dll(
        Cs.Dependency("Microsoft.Bcl.Immutable"),
        Cs.Dependency("Newtonsoft.Json"),
        Cs.Dependency("NuGet.Core"),
        Cs.Dependency("Antlr4.StringTemplate"),
        CSharpKeys.RootNamespace.Modify("Bud")
      ), Cs.Test(
        Cs.Dependency("NUnit"),
        CSharpKeys.RootNamespace.Modify("Bud")
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