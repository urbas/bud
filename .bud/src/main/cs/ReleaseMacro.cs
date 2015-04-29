using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bud;
using Bud.Build;
using Bud.Cli;
using Bud.CSharp;
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
    await context.Evaluate("clean");
    await context.Evaluate("publish");
    await context.Evaluate("project/bud/main/cs/dist");
    var budZipFileName = CreateBudZipFile(context, version);
    UploadToChocolatey(context, version);
    UploadZipToServer(context, budZipFileName);
  }

  private static string CreateBudZipFile(IConfig context, SemanticVersion version) {
    var budZipFileName = string.Format("bud-{0}.zip", version);
    using (new TemporaryChangeDir(GetBudDistDir(context))) {
      ProcessBuilder.Execute("zip", "-r", budZipFileName, ".");
    }
    return budZipFileName;
  }

  private static string GetBudDistDir(IConfig context) {
    return context.GetDistDir(Key.Parse("/project/bud/main/cs"));
  }

  private static void UploadToChocolatey(IConfig context, SemanticVersion version) {
    using (new TemporaryChangeDir(GetChocolateySpecDir(context))) {
      ProcessBuilder.Execute("cpack");
      ProcessBuilder.Execute("cpush", "-r", string.Format("bud.{0}.nupkg", version));
    }
  }

  private static void UploadZipToServer(IConfig context, string budZipFileName) {
    var baseDir = context.GetBaseDir();
    using (new TemporaryChangeDir(baseDir)) {
      var sshKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "budpage_id_rsa");
      var relativeZipPath = MakeRelative(Path.Combine(GetBudDistDir(context), budZipFileName), baseDir + "/");
      ProcessBuilder.Execute("scp", "-i", sshKeyPath, relativeZipPath, string.Format("budpage@54.154.215.159:/home/budpage/production-budpage/shared/public/packages/{0}", budZipFileName));
    }
  }

  public static string MakeRelative(string filePath, string referencePath) {
    var fileUri = new Uri(filePath);
    var referenceUri = new Uri(referencePath);
    return referenceUri.MakeRelativeUri(fileUri).ToString();
  }

  private static string GetChocolateySpecDir(IConfig context) {
    return Path.Combine(context.GetBaseDir(), "DevelopmentUtils", "ChocolateyPackage");
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
    ReplaceLinesInFile(Path.Combine(GetChocolateySpecDir(buildContext), "tools", "chocolateyInstall.ps1"), line => versionMatcher.Replace(line, versionReplacement));
  }

  private static void UpdateNuspecVersion(BuildContext buildContext, SemanticVersion version) {
    var versionMatcher = new Regex(@"<version>.+?</version>");
    var versionReplacement = string.Format("<version>{0}</version>", version);
    ReplaceLinesInFile(Path.Combine(GetChocolateySpecDir(buildContext), "bud.nuspec"), line => versionMatcher.Replace(line, versionReplacement));
  }

  private static void ReplaceLinesInFile(string file, Func<string, string> lineReplacer) {
    File.WriteAllLines(file, File.ReadAllLines(file).Select(lineReplacer).ToArray());
  }

  private static string GetAssemblyInfoFile(BuildContext buildContext, string projectId) {
    var mainSourcesDir = buildContext.Config.GetBaseDir(Key.Parse(string.Format("/project/{0}/main/cs", projectId)));
    return Path.Combine(mainSourcesDir, "Properties", "AssemblyInfo.cs");
  }
}

public class TemporaryChangeDir : IDisposable {
  private readonly string OldWorkingDir;

  public TemporaryChangeDir(string dir) {
    OldWorkingDir = Directory.GetCurrentDirectory();
    Directory.SetCurrentDirectory(dir);
  }

  public void Dispose() {
    Directory.SetCurrentDirectory(OldWorkingDir);
  }
}