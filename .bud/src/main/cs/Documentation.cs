using System;
using System.IO;
using System.Text.RegularExpressions;
using Bud;
using Bud.Build;
using NuGet;

internal class Documentation {
  public static void PrepareDocsForRelease(IContext context, SemanticVersion releaseVersion) {
    PrepareReadmeForRelease(context, releaseVersion);
  }

  private static void PrepareReadmeForRelease(IConfig config, SemanticVersion releaseVersion) {
    var readmeFile = Path.Combine(config.GetBaseDir(), "README.md");
    FileProcessingUtils.ReplaceLinesInFile(readmeFile,
                                           LatestVersionEntryReplacer(releaseVersion),
                                           GithubReleaseTagReplacer(releaseVersion),
                                           PortableDistributionReplacer(releaseVersion),
                                           ChocolateyDistributionReplacer(releaseVersion),
                                           UbuntuDistributionReplacer(releaseVersion));
  }

  private static Func<string, string> LatestVersionEntryReplacer(SemanticVersion releaseVersion) {
    return RegexVersionReplacement(releaseVersion,
                                   @"__Latest version__: \[.+?\]",
                                   "__Latest version__: [{0}]");
  }

  private static Func<string, string> GithubReleaseTagReplacer(SemanticVersion releaseVersion) {
    return RegexVersionReplacement(releaseVersion,
                                   @"\(https://github.com/urbas/bud/releases/tag/v.+?\)",
                                   "(https://github.com/urbas/bud/releases/tag/v{0})");
  }

  private static Func<string, string> PortableDistributionReplacer(SemanticVersion releaseVersion) {
    return RegexVersionReplacement(releaseVersion,
                                   @"\[bud-.+?.zip\]\(https://dl.dropboxusercontent.com/u/9516950/bud/bud-.+?.zip\)",
                                   "[bud-{0}.zip](https://dl.dropboxusercontent.com/u/9516950/bud/bud-{0}.zip)");
  }

  private static Func<string, string> UbuntuDistributionReplacer(SemanticVersion releaseVersion) {
    return RegexVersionReplacement(releaseVersion,
                                   @"\[bud_.+?_i386.deb\]\(https://dl.dropboxusercontent.com/u/9516950/bud/bud_.+?_i386.deb\)",
                                   "[bud_{0}_i386.deb](https://dl.dropboxusercontent.com/u/9516950/bud/bud_{0}_i386.deb)");
  }

  private static Func<string, string> ChocolateyDistributionReplacer(SemanticVersion releaseVersion) {
    return RegexVersionReplacement(releaseVersion,
                                   @"\(https://chocolatey.org/packages/bud/.+?\)",
                                   "(https://chocolatey.org/packages/bud/{0})");
  }

  private static Func<string, string> RegexVersionReplacement(SemanticVersion releaseVersion, string matchString, string replacementString) {
    var matcher = new Regex(matchString);
    var replacement = string.Format(replacementString, releaseVersion);
    return line => matcher.Replace(line, replacement);
  }
}