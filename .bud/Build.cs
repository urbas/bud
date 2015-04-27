using System.IO;
using Bud;
using Bud.Build;
using Bud.CSharp;
using Bud.Projects;
using Bud.Resources;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    var budCore = new Project("Bud.Core",
                              Res.Main(),
                              Cs.Dll(Cs.RootNamespace.Modify("Bud"),
                                     Cs.Dependency("Microsoft.Bcl.Immutable"),
                                     Cs.Dependency("Newtonsoft.Json"),
                                     Cs.Dependency("NuGet.Core"),
                                     Cs.Dependency("Antlr4.StringTemplate"),
                                     Cs.Dependency("Bud.NUnit.ConsoleRunner")),
                              Cs.Test(Cs.RootNamespace.Modify("Bud"),
                                      Cs.Dependency("NUnit")));

    var bud = new Project("bud",
                          Cs.Exe(Cs.RootNamespace.Modify("Bud"),
                                 Cs.Dependency("Bud.Core"),
                                 Cs.Dependency("CommandLineParser")));

    var budTest = new Project("Bud.Test", Cs.Dll(Cs.Dependency("Bud.Core"), Cs.Dependency("NUnit")));

    var budSystemTests = new Project("Bud.SystemTests",
                                     Cs.Test(Cs.Dependency("Bud.Test"),
                                             NUnitPlugin.NUnitArgs.Modify(list => list.Add("/noshadow"))));

    var budExamplesSnippets = new Project("Bud.Examples.Snippets", Cs.Dll(Cs.Dependency("Bud.Core")));

    return settings.AddGlobally(Cs.TargetFramework.Init(Framework.Net46))
                   .Version("0.1.3-dev")
                   .Add(bud, budCore, budTest, budSystemTests, budExamplesSnippets);
  }
}