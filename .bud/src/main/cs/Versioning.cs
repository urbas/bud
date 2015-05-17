using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Bud;
using Bud.Build;
using NuGet;

internal static class Versioning {
  public static void SetReleaseVersion(IBuildContext buildContext, SemanticVersion releaseVersion) {
    File.WriteAllText(Build.GetVersionFile(buildContext.Config), releaseVersion.ToString());
    UpdateAssemblyInfoVersion(buildContext, Build.BudCoreProjectId, releaseVersion);
    UpdateAssemblyInfoVersion(buildContext, Build.BudProjectId, releaseVersion);
    UpdateNuspecVersion(buildContext, releaseVersion);
    UpdateDistZipUrl(buildContext, releaseVersion);
    buildContext.Reset();
  }

  public static void SetNextDevelopmentVersion(IBuildContext context, SemanticVersion nextDevelopmentVersion) {
    SetReleaseVersion(context, nextDevelopmentVersion);
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

  public static void UpdateAssemblyInfoVersion(IBuildContext buildContext, string projectId, SemanticVersion version) {
    var versionMatcher = new Regex(@"(?<prefix>AssemblyVersion\s*\("").*?(?<suffix>""\))");
    var versionReplacement = string.Format("${{prefix}}{0}.{1}.{2}${{suffix}}", version.Version.Major, version.Version.Minor, version.Version.Build);
    var productMatcher = new Regex(@"(?<prefix>AssemblyProduct\s*\("").*?(?<suffix>""\))");
    var productReplacement = string.Format("${{prefix}}{0} v{1}${{suffix}}", projectId, version);
    var assemblyInfoFile = GetAssemblyInfoFile(buildContext, projectId);
    ReplaceLinesInFile(assemblyInfoFile,
                       line => productMatcher.Replace(versionMatcher.Replace(line, versionReplacement), productReplacement));
  }

  public static void UpdateDistZipUrl(IBuildContext buildContext, SemanticVersion version) {
    var versionMatcher = new Regex(@"bud-.+?\.zip");
    var versionReplacement = string.Format("bud-{0}.zip", version);
    var chocolateySpecDir = ChocolateyPackaging.GetChocolateySpecDir(buildContext.Config);
    var installationScriptTemplate = Path.Combine(chocolateySpecDir, "tools", "chocolateyInstall.ps1");
    ReplaceLinesInFile(installationScriptTemplate,
                       line => versionMatcher.Replace(line, versionReplacement));
  }

  public static void UpdateNuspecVersion(IBuildContext buildContext, SemanticVersion version) {
    var versionMatcher = new Regex(@"<version>.+?</version>");
    var versionReplacement = string.Format("<version>{0}</version>", version);
    var chocolateySpecDir = ChocolateyPackaging.GetChocolateySpecDir(buildContext.Config);
    var budNuspecTemplate = Path.Combine(chocolateySpecDir, "bud.nuspec");
    ReplaceLinesInFile(budNuspecTemplate,
                       line => versionMatcher.Replace(line, versionReplacement));
  }

  public static void ReplaceLinesInFile(string file, Func<string, string> lineReplacer) {
    var replacedLines = File.ReadAllLines(file).Select(lineReplacer);
    File.WriteAllLines(file, replacedLines);
  }

  public static string GetAssemblyInfoFile(IBuildContext buildContext, string projectId) {
    var mainSourcesDir = buildContext.Config.GetBaseDir(Key.Parse(string.Format("/project/{0}/main/cs", projectId)));
    return Path.Combine(mainSourcesDir, "Properties", "AssemblyInfo.cs");
  }
}