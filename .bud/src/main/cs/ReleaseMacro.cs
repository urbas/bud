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
        Versioning.SetVersion(context, releaseVersion);
        GitTasks.GitTagRelease(releaseVersion);
        Publish(context.Context, releaseVersion).Wait();
        Versioning.SetVersion(context, nextDevelopmentVersion);
        GitTasks.GitCommitNextDevelopmentVersion(nextDevelopmentVersion);
    }

    private static async Task Publish(IContext context, SemanticVersion version) {
        await BuildBudDistFiles(context);
        await PublishNuGetPackages(context);
        ChocolateyPackaging.PublishToChocolatey(context, version);
        UbuntuPackaging.CreateUbuntuPackage(context, version);
    }

    private static async Task PublishNuGetPackages(IContext context) {
        await context.Evaluate("publish");
    }

    private static async Task BuildBudDistFiles(IContext context) {
        await context.Evaluate("clean");
        await context.Evaluate("project/bud/main/cs/dist");
    }
}