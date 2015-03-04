using Bud;
using Bud.CSharp;
using Bud.Projects;
using Bud.Publishing;
using Bud.SolutionExporter;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings
      .Globally(CSharpKeys.TargetFramework.Init(Framework.Net46))
      .Version("0.0.2")
      .GenerateSolution()
      .Project("bud", Path.Combine(baseDir, "bud"), Cs.Exe(
        Cs.Dependency("Bud.Core"),
        CSharpKeys.RootNamespace.Modify("Bud")
      ))
      .Project("Bud.Core", Path.Combine(baseDir, "Core"), Cs.Dll(
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
        Cs.Dependency("Bud.Test"),
        CSharpKeys.RootNamespace.Modify("Bud")
      ));
  }
}