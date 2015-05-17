using System;
using System.Threading.Tasks;
using Bud;
using Bud.Projects;
using CommandLine;
using NuGet;

public static class ReleaseMacro {
  public static MacroResult PerformRelease(IBuildContext context, string[] commandLineArgs) {
    var cliArguments = new PerformReleaseArguments();
    if (Parser.Default.ParseArguments(commandLineArgs, cliArguments)) {
      var releaseVersion = SemanticVersion.Parse(cliArguments.Version);
      var nextDevelopmentVersion = Versioning.GetNextDevelopmentVersion(cliArguments, releaseVersion);
      PerformRelease(context, releaseVersion, nextDevelopmentVersion);
    }
    return new MacroResult(context.Config.Evaluate(Key.Root / ProjectKeys.Version), context);
  }

  private static void PerformRelease(IBuildContext context, SemanticVersion releaseVersion, SemanticVersion nextDevelopmentVersion) {
    Versioning.SetSourcesVersion(context, releaseVersion);
    Documentation.PrepareDocsForRelease(context.Context, releaseVersion);
    GitTasks.GitTagRelease(releaseVersion);
    Publish(context.Context, releaseVersion).Wait();
    Versioning.SetSourcesVersion(context, nextDevelopmentVersion);
    GitTasks.GitCommitNextDevelopmentVersion(nextDevelopmentVersion);
  }

  public static Macro Create(string macroName, Action<IContext, SemanticVersion> releaseAction) {
    return new Macro(macroName, (buildContext, cliArgs) => PerformReleaseAction(buildContext, cliArgs, releaseAction));
  }

  public static MacroResult PerformReleaseAction(IBuildContext buildContext, string[] cliArgs, Action<IContext, SemanticVersion> releaseAction) {
    var parsedArgs = new PerformReleaseArguments();
    if (Parser.Default.ParseArguments(cliArgs, parsedArgs)) {
      var version = SemanticVersion.Parse(parsedArgs.Version);
      releaseAction(buildContext.Context, version);
    }
    return new MacroResult(null, buildContext);
  }

  private static async Task Publish(IContext context, SemanticVersion version) {
    await context.Evaluate("clean");
    await context.Evaluate("publish");
    DistributionZipPackaging.UploadDistZip(context, version);
    ChocolateyPackaging.PublishToChocolatey(context, version);
    UbuntuPackaging.CreateUbuntuPackage(context, version);
  }
}