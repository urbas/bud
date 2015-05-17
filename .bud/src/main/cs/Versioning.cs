using System.IO;
using System.Text.RegularExpressions;
using Bud;
using Bud.Build;
using NuGet;

internal static class Versioning {
  public static void SetSourcesVersion(IBuildContext buildContext, SemanticVersion version) {
    File.WriteAllText(Build.GetVersionFile(buildContext.Config), version.ToString());
    UpdateAssemblyInfoVersion(buildContext, Build.BudCoreProjectId, version);
    UpdateAssemblyInfoVersion(buildContext, Build.BudProjectId, version);
    UpdateNuspecVersion(buildContext, version);
    UpdateDistZipUrl(buildContext, version);
    buildContext.Reset();
  }

  public static SemanticVersion GetNextDevelopmentVersion(PerformReleaseArguments cliArguments, SemanticVersion releaseVersion) {
    return string.IsNullOrEmpty(cliArguments.NextDevelopmentVersion) ? GetNextDevelopmentVersion(releaseVersion) :
      SemanticVersion.Parse(cliArguments.NextDevelopmentVersion);
  }

  public static SemanticVersion GetNextDevelopmentVersion(SemanticVersion releaseVersion) {
    return new SemanticVersion(releaseVersion.Version.Major,
                               releaseVersion.Version.Minor,
                               releaseVersion.Version.Build + 1,
                               "dev");
  }

  private static void UpdateAssemblyInfoVersion(IBuildContext buildContext, string projectId, SemanticVersion version) {
    var versionMatcher = new Regex(@"(?<prefix>AssemblyVersion\s*\("").*?(?<suffix>""\))");
    var versionReplacement = string.Format("${{prefix}}{0}.{1}.{2}${{suffix}}", version.Version.Major, version.Version.Minor, version.Version.Build);
    var productMatcher = new Regex(@"(?<prefix>AssemblyProduct\s*\("").*?(?<suffix>""\))");
    var productReplacement = string.Format("${{prefix}}{0} v{1}${{suffix}}", projectId, version);
    var assemblyInfoFile = GetAssemblyInfoFile(buildContext, projectId);
    FileProcessingUtils.ReplaceLinesInFile(assemblyInfoFile,
                                           line => versionMatcher.Replace(line, versionReplacement),
                                           line => productMatcher.Replace(line, productReplacement));
  }

  public static void UpdateDistZipUrl(IBuildContext buildContext, SemanticVersion version) {
    var versionMatcher = new Regex(@"bud-.+?\.zip");
    var versionReplacement = string.Format("bud-{0}.zip", version);
    var chocolateySpecDir = ChocolateyPackaging.GetChocolateySpecDir(buildContext.Config);
    var installationScriptTemplate = Path.Combine(chocolateySpecDir, "tools", "chocolateyInstall.ps1");
    FileProcessingUtils.ReplaceLinesInFile(installationScriptTemplate,
                                           line => versionMatcher.Replace(line, versionReplacement));
  }

  public static void UpdateNuspecVersion(IBuildContext buildContext, SemanticVersion version) {
    var versionMatcher = new Regex(@"<version>.+?</version>");
    var versionReplacement = string.Format("<version>{0}</version>", version);
    var chocolateySpecDir = ChocolateyPackaging.GetChocolateySpecDir(buildContext.Config);
    var budNuspecTemplate = Path.Combine(chocolateySpecDir, "bud.nuspec");
    FileProcessingUtils.ReplaceLinesInFile(budNuspecTemplate,
                                           line => versionMatcher.Replace(line, versionReplacement));
  }

  public static string GetAssemblyInfoFile(IBuildContext buildContext, string projectId) {
    var mainSourcesDir = buildContext.Config.GetBaseDir(Key.Parse(string.Format("/project/{0}/main/cs", projectId)));
    return Path.Combine(mainSourcesDir, "Properties", "AssemblyInfo.cs");
  }
}