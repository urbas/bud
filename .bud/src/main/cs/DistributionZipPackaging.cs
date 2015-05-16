using System;
using System.IO;
using Bud;
using Bud.Cli;
using Bud.IO;
using CommandLine;
using NuGet;

internal static class DistributionZipPackaging {
  public static readonly string DropboxDistributionDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Dropbox", "Public", "bud");

  public static void UploadDistZip(IConfig context, SemanticVersion version) {
    var budDistZipFile = CreateBudDistZip(context, version);
    PlaceZipIntoDropbox(context, budDistZipFile);
  }

  public static MacroResult UploadDistZip(IBuildContext context, string[] cliArgs) {
    var parsedArgs = new PerformReleaseArguments();
    if (Parser.Default.ParseArguments(cliArgs, parsedArgs)) {
      var version = SemanticVersion.Parse(parsedArgs.Version);
      UploadDistZip(context.Config, version);
    }
    return new MacroResult(null, context);
  }

  public static string CreateBudDistZip(IConfig context, SemanticVersion version) {
    var budZipFileName = string.Format("bud-{0}.zip", version);
    var budDistDir = Build.GetBudDistDir(context);
    using (new TemporaryDirChange(budDistDir)) {
      ProcessBuilder.Execute("zip", "-r", budZipFileName, ".");
    }
    return Path.Combine(budDistDir, budZipFileName);
  }

  public static void PlaceZipIntoDropbox(IConfig config, string budZipDistFile) {
    var dropBoxDestination = Path.Combine(DropboxDistributionDir, Path.GetFileName(budZipDistFile));
    File.Copy(budZipDistFile, dropBoxDestination);
  }
}