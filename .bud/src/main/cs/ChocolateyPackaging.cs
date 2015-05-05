using System;
using System.IO;
using Bud;
using Bud.Build;
using Bud.Cli;
using Bud.IO;
using NuGet;

static internal class ChocolateyPackaging {
  public static void PublishToChocolatey(IConfig context, SemanticVersion version) {
    var budZipFileName = CreateBudDistZip(context, version);
    UploadChocolateyNupkg(context, version);
    UploadZipToServer(context, budZipFileName);
  }

  public static string CreateBudDistZip(IConfig context, SemanticVersion version) {
    var budZipFileName = String.Format("bud-{0}.zip", version);
    using (new TemporaryDirChange(Build.GetBudDistDir(context))) {
      ProcessBuilder.Execute("zip", "-r", budZipFileName, ".");
    }
    return budZipFileName;
  }

  public static void UploadChocolateyNupkg(IConfig context, SemanticVersion version) {
    using (new TemporaryDirChange(GetChocolateySpecDir(context))) {
      ProcessBuilder.Execute("cpack");
      ProcessBuilder.Execute("cpush", "-r", string.Format("bud.{0}.nupkg", version));
    }
  }

  public static void UploadZipToServer(IConfig context, string budZipFileName) {
    var baseDir = context.GetBaseDir();
    using (new TemporaryDirChange(baseDir)) {
      var sshKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "budpage_id_rsa");
      var relativeZipPath = Paths.MakeRelative(Path.Combine(Build.GetBudDistDir(context), budZipFileName), baseDir + "/");
      ProcessBuilder.Execute("scp", "-i", sshKeyPath, relativeZipPath, string.Format("budpage@54.154.215.159:/home/budpage/production-budpage/shared/public/packages/{0}", budZipFileName));
    }
  }

  public static string GetChocolateySpecDir(IConfig context) {
    return Path.Combine(context.GetBaseDir(), "DevelopmentUtils", "ChocolateyPackage");
  }
}