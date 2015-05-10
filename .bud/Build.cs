using System.IO;
using Bud;
using Bud.Build;
using Bud.CSharp;
using Bud.Projects;
using Bud.Resources;
using NuGet;
using Settings = Bud.Settings;

public class Build : IBuild {
  public const string BudCoreProjectId = "Bud.Core";
  public const string BudProjectId = "bud";

  public Settings Setup(Settings settings, string baseDir) {
    var budCore = new Project(BudCoreProjectId,
                              Res.Main(),
                              Cs.Dll(Cs.RootNamespace.Modify("Bud"),
                                     Cs.Dependency("Microsoft.Bcl.Immutable"),
                                     Cs.Dependency("Newtonsoft.Json"),
                                     Cs.Dependency("NuGet.Core"),
                                     Cs.Dependency("Antlr4.StringTemplate"),
                                     Cs.Dependency("Bud.NUnit.ConsoleRunner"),
                                     Cs.Dependency("CommandLineParser")),
                              Cs.Test(Cs.RootNamespace.Modify("Bud"),
                                      Cs.Dependency("NUnit")));

    var bud = new Project(BudProjectId,
                          Cs.Exe(Cs.RootNamespace.Modify("Bud"),
                                 Cs.Dependency(BudCoreProjectId)));

    var budTest = new Project("Bud.Test", Cs.Dll(Cs.Dependency(BudCoreProjectId), Cs.Dependency("NUnit")));

    var budSystemTests = new Project("Bud.SystemTests",
                                     Cs.Test(Cs.Dependency("Bud.Test"),
                                             NUnitPlugin.NUnitArgs.Modify(list => list.Add("/noshadow"))));

    var budExamplesSnippets = new Project("Bud.Examples.Snippets", Cs.Dll(Cs.Dependency(BudCoreProjectId)));

    return settings.AddGlobally(Cs.TargetFramework.Init(Framework.Net46))
                   .Add(ProjectKeys.Version.Modify(ReadFromBudCoreVersionResourceFile))
                   .Add(bud, budCore, budTest, budSystemTests, budExamplesSnippets)
                   .Add(new Macro("performRelease", ReleaseMacro.PerformRelease))
                   .Add(new Macro("createUbuntuPackage", UbuntuPackaging.CreateUbuntuPackageMacro));
  }

  private static SemanticVersion ReadFromBudCoreVersionResourceFile(IConfig config) {
    var versionFile = GetVersionFile(config);
    var version = File.ReadAllText(versionFile).Trim();
    return SemanticVersion.Parse(version);
  }

  public static string GetVersionFile(IConfig config) {
    return Path.Combine(GetBudCoreProjectBaseDir(config), "version");
  }

  private static string GetBudCoreProjectBaseDir(IConfig config) {
    return config.GetBaseDir(Project.ProjectKey(BudCoreProjectId) / "main/resources");
  }

  public static string GetBudDistDir(IConfig context) {
    return context.GetDistDir(Key.Parse("/project/bud/main/cs"));
  }
}