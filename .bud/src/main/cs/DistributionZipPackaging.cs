using System;
using System.IO;
using System.Threading.Tasks;
using Bud;
using Bud.Cli;
using Bud.IO;
using NuGet;

internal static class DistributionZipPackaging {
  public static readonly string DropboxDistributionDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Dropbox", "Public", "bud");

  public static async void UploadDistZip(IConfig context, SemanticVersion version) {
    var budDistZipFile = await CreateBudDistZip(context, version);
    PlaceZipIntoDropbox(context, budDistZipFile);
  }

  public static async Task<string> CreateBudDistZip(IConfig context, SemanticVersion version) {
    await context.Evaluate("project/bud/main/cs/dist");
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