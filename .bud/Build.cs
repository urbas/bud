using System.Collections.Generic;
using Bud;
using Bud.CSharp;
using Bud.Projects;
using Bud.Resources;
using System.IO;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings
      .AddGlobally(CSharpKeys.TargetFramework.Init(Framework.Net46))
      .Version("0.1.0")
      .Project("bud", Path.Combine(baseDir, "bud"), Cs.Exe(
        CSharpKeys.RootNamespace.Modify("Bud"),
        Cs.Dependency("Bud.Core"),
        Cs.Dependency("CommandLineParser")
      ))
      .Project("Bud.Core", Path.Combine(baseDir, "Core"), Res.Main(), Cs.Dll(
        CSharpKeys.RootNamespace.Modify("Bud"),
        Cs.Dependency("Microsoft.Bcl.Immutable"),
        Cs.Dependency("Newtonsoft.Json"),
        Cs.Dependency("NuGet.Core"),
        Cs.Dependency("Antlr4.StringTemplate"),
        Cs.Dependency("Bud.NUnit.ConsoleRunner")
      ), Cs.Test(
        CSharpKeys.RootNamespace.Modify("Bud"),
        Cs.Dependency("NUnit")
      ))
      .Project("Bud.Test", Path.Combine(baseDir, "Test"), Cs.Dll(
        Cs.Dependency("Bud.Core"),
        Cs.Dependency("NUnit")
      ))
      .Project("Bud.SystemTests", Path.Combine(baseDir, "SystemTests"), Cs.Test(
        Cs.Dependency("Bud.Test"),
        NUnitTestTargetPlugin.NUnitArgumentsKey.Modify(RunTestsWithouShadowDlls)
      ))
      .Project("Bud.Examples.Snippets", Path.Combine(baseDir, "Bud.Examples.Snippets"), Cs.Dll(
        Cs.Dependency("Bud.Core")
      ));
  }

  private static List<string> RunTestsWithouShadowDlls(List<string> oldArgs) {
    oldArgs.Add("/noshadow");
    return oldArgs;
  }
}