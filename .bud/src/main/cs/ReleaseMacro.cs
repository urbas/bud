using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bud;
using Bud.Build;
using Bud.Cli;
using Bud.Projects;
using CommandLine;
using NuGet;

public static class ReleaseMacro {
  public static MacroResult PerformRelease(BuildContext context, string[] commandLineArgs) {
    var cliArguments = new PerformReleaseArguments();
    if (Parser.Default.ParseArguments(commandLineArgs, cliArguments)) {
      var releaseVersion = SemanticVersion.Parse(cliArguments.Version);
      var nextDevelopmentVersion = GetNextDevelopmentVersion(cliArguments, releaseVersion);
      PerformRelease(context, releaseVersion, nextDevelopmentVersion);
    }
    return new MacroResult(context.Config.Evaluate(Key.Root / ProjectKeys.Version), context);
  }

  private static void PerformRelease(BuildContext context, SemanticVersion releaseVersion, SemanticVersion nextDevelopmentVersion) {
    SetVersion(context, releaseVersion);
    GitTagRelease(releaseVersion);
    Publish(context, releaseVersion).Wait();
    SetVersion(context, nextDevelopmentVersion);
    GitCommitNextDevelopmentVersion(nextDevelopmentVersion);
  }

  private static void SetVersion(BuildContext buildContext, SemanticVersion releaseVersion) {
    File.WriteAllText(Build.GetVersionFile(buildContext.Config), releaseVersion.ToString());
    UpdateAssemblyInfoVersion(buildContext, Build.BudCoreProjectId, releaseVersion);
    UpdateAssemblyInfoVersion(buildContext, Build.BudProjectId, releaseVersion);
    UpdateNuspecVersion(buildContext, releaseVersion);
    UpdateDistZipUrl(buildContext, releaseVersion);
    buildContext.ReloadConfig();
  }

  private static void GitTagRelease(SemanticVersion releaseVersion) {
    ProcessBuilder.Execute("git", "commit", "-am", string.Format("Release {0}", releaseVersion));
    ProcessBuilder.Execute("git", "tag", "v" + releaseVersion);
  }

  private static async Task Publish(IConfig context, SemanticVersion version) {
    await BuildBudDistFiles(context);
    await PublishNuGetPackages(context);
    ChocolateyPackaging.PublishToChocolatey(context, version);
    UbuntuPackaging.CreateUbuntuPackage(context, version);
  }

  private static async Task PublishNuGetPackages(IConfig context) {
    await context.Evaluate("publish");
  }

  private static async Task BuildBudDistFiles(IConfig context) {
    await context.Evaluate("clean");
    await context.Evaluate("project/bud/main/cs/dist");
  }

  private static void GitCommitNextDevelopmentVersion(SemanticVersion nextDevelopmentVersion) {
    ProcessBuilder.Execute("git", "commit", "-am", string.Format("Setting next development version: {0}", nextDevelopmentVersion));
  }

  private static SemanticVersion GetNextDevelopmentVersion(PerformReleaseArguments cliArguments, SemanticVersion releaseVersion) {
    return string.IsNullOrEmpty(cliArguments.NextDevelopmentVersion) ?
      GetNextDevelopmentVersion(releaseVersion) :
      SemanticVersion.Parse(cliArguments.NextDevelopmentVersion);
  }

  private static SemanticVersion GetNextDevelopmentVersion(SemanticVersion releaseVersion) {
    return new SemanticVersion(releaseVersion.Version.Major,
                               releaseVersion.Version.Minor,
                               releaseVersion.Version.Build + 1,
                               "dev");
  }

  private static void UpdateAssemblyInfoVersion(BuildContext buildContext, string projectId, SemanticVersion version) {
    var versionMatcher = new Regex(@"(?<prefix>AssemblyVersion\s*\("").*?(?<suffix>""\))");
    var versionReplacement = string.Format("${{prefix}}{0}.{1}.{2}${{suffix}}", version.Version.Major, version.Version.Minor, version.Version.Build);
    var productMatcher = new Regex(@"(?<prefix>AssemblyProduct\s*\("").*?(?<suffix>""\))");
    var productReplacement = string.Format("${{prefix}}{0} v{1}${{suffix}}", projectId, version);
    ReplaceLinesInFile(GetAssemblyInfoFile(buildContext, projectId), line => productMatcher.Replace(versionMatcher.Replace(line, versionReplacement), productReplacement));
  }

  private static void UpdateDistZipUrl(BuildContext buildContext, SemanticVersion version) {
    var versionMatcher = new Regex(@"bud-.+?\.zip");
    var versionReplacement = string.Format("bud-{0}.zip", version);
    ReplaceLinesInFile(Path.Combine(ChocolateyPackaging.GetChocolateySpecDir(buildContext), "tools", "chocolateyInstall.ps1"), line => versionMatcher.Replace(line, versionReplacement));
  }

  private static void UpdateNuspecVersion(BuildContext buildContext, SemanticVersion version) {
    var versionMatcher = new Regex(@"<version>.+?</version>");
    var versionReplacement = string.Format("<version>{0}</version>", version);
    ReplaceLinesInFile(Path.Combine(ChocolateyPackaging.GetChocolateySpecDir(buildContext), "bud.nuspec"), line => versionMatcher.Replace(line, versionReplacement));
  }

  private static void ReplaceLinesInFile(string file, Func<string, string> lineReplacer) {
    File.WriteAllLines(file, File.ReadAllLines(file).Select(lineReplacer).ToArray());
  }

  private static string GetAssemblyInfoFile(BuildContext buildContext, string projectId) {
    var mainSourcesDir = buildContext.Config.GetBaseDir(Key.Parse(string.Format("/project/{0}/main/cs", projectId)));
    return Path.Combine(mainSourcesDir, "Properties", "AssemblyInfo.cs");
  }
}