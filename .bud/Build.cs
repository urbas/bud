using System.IO;
using Bud;
using Bud.CSharp;
using Bud.Projects;
using Bud.Resources;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    var budCore = new Project("Bud.Core",
                              Path.Combine(baseDir, "Core"),
                              Res.Main(),
                              Cs.Dll(CSharpKeys.RootNamespace.Modify("Bud"),
                                     Cs.Dependency("Microsoft.Bcl.Immutable"),
                                     Cs.Dependency("Newtonsoft.Json"),
                                     Cs.Dependency("NuGet.Core"),
                                     Cs.Dependency("Antlr4.StringTemplate"),
                                     Cs.Dependency("Bud.NUnit.ConsoleRunner")),
                              Cs.Test(CSharpKeys.RootNamespace.Modify("Bud"),
                                      Cs.Dependency("NUnit")));

    var bud = new Project("bud",
                          Path.Combine(baseDir, "bud"),
                          Cs.Exe(CSharpKeys.RootNamespace.Modify("Bud"),
                                 Cs.Dependency("Bud.Core"),
                                 Cs.Dependency("CommandLineParser")));

    var budTest = new Project("Bud.Test",
                              Path.Combine(baseDir, "Test"),
                              Cs.Dll(Cs.Dependency("Bud.Core"),
                                     Cs.Dependency("NUnit")));

    var budSystemTests = new Project("Bud.SystemTests",
                                     Path.Combine(baseDir, "SystemTests"),
                                     Cs.Test(Cs.Dependency("Bud.Test"),
                                             NUnitPlugin.NUnitArgs.Modify(list => list.Add("/noshadow"))));

    var budExamplesSnippets = new Project("Bud.Examples.Snippets",
                                          Path.Combine(baseDir, "Bud.Examples.Snippets"),
                                          Cs.Dll(Cs.Dependency("Bud.Core")));

    return settings.AddGlobally(CSharpKeys.TargetFramework.Init(Framework.Net46))
                   .Version("0.1.2")
                   .Add(bud, budCore, budTest, budSystemTests, budExamplesSnippets);
  }
}