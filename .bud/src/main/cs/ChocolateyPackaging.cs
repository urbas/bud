using System;
using System.IO;
using Bud;
using Bud.Build;
using Bud.Cli;
using Bud.IO;
using CommandLine;
using NuGet;

internal static class ChocolateyPackaging {
  public static void PublishToChocolatey(IConfig context, SemanticVersion version) {
    var budDistZipFile = CreateBudDistZip(context, version);
    PlaceZipIntoDropbox(context, budDistZipFile);
    UploadChocolateyNupkg(context, version);
  }

  public static string CreateBudDistZip(IConfig context, SemanticVersion version) {
    var budZipFileName = string.Format("bud-{0}.zip", version);
    var budDistDir = Build.GetBudDistDir(context);
    using (new TemporaryDirChange(budDistDir)) {
      ProcessBuilder.Execute("zip", "-r", budZipFileName, ".");
    }
    return Path.Combine(budDistDir, budZipFileName);
  }

  public static void UploadChocolateyNupkg(IConfig context, SemanticVersion version) {
    using (new TemporaryDirChange(GetChocolateySpecDir(context))) {
      ProcessBuilder.Execute("cpack");
      ProcessBuilder.Execute("cpush", "-r", string.Format("bud.{0}.nupkg", version));
    }
  }

  public static void PlaceZipIntoDropbox(IConfig config, string budZipDistFile) {
    var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    var dropBoxDestination = Path.Combine(homeFolder, "Dropbox", "Public", "bud", Path.GetFileName(budZipDistFile));
    File.Copy(budZipDistFile, dropBoxDestination);
  }

  public static string GetChocolateySpecDir(IConfig config) {
    return Path.Combine(config.GetBaseDir(), "DevelopmentUtils", "ChocolateyPackage");
  }

  public static MacroResult UploadDistZip(IBuildContext context, string[] cliArgs) {
    var parsedArgs = new PerformReleaseArguments();
    if (Parser.Default.ParseArguments(cliArgs, parsedArgs)) {
      var releaseVersion = SemanticVersion.Parse(parsedArgs.Version);
      var budDistZipFile = CreateBudDistZip(context.Config, releaseVersion);
      PlaceZipIntoDropbox(context.Config, budDistZipFile);
    }
    return new MacroResult(null, context);
  }
}