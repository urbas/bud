using System;
using System.IO;
using System.Threading.Tasks;
using Bud;
using Bud.Cli;
using Bud.IO;
using NuGet;

internal static class DistributionZipPackaging {
  public static readonly string DropboxDistributionDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Dropbox", "Public", "bud");

  public static void UploadDistZip(IConfig context, SemanticVersion version) {
    context.Evaluate("project/bud/main/cs/dist").Wait();
    var budZipFileName = string.Format("bud-{0}.zip", version);
    var budDistDir = Build.GetBudDistDir(context);
    using (var scratchDir = new TemporaryDirectory(budDistDir)) {
      using (new TemporaryDirChange(scratchDir.Path)) {
        ProcessBuilder.Execute("zip", "-r", budZipFileName, ".");
        PlaceZipIntoDropbox(context, Path.Combine(scratchDir.Path, budZipFileName));
      }
    }
  }

  public static void PlaceZipIntoDropbox(IConfig config, string budZipDistFile) {
    var dropBoxDestination = Path.Combine(DropboxDistributionDir, Path.GetFileName(budZipDistFile));
    File.Copy(budZipDistFile, dropBoxDestination);
  }
}