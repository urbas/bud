using System.IO;
using Bud;
using Bud.Cli;
using Bud.IO;
using NuGet;

public static class ChocolateyPackaging {
  public static void PublishToChocolatey(IConfig context, SemanticVersion version) {
    using (var tempDir = new TemporaryDirectory(GetChocolateySpecDir(context))) {
      using (new TemporaryDirChange(tempDir.Path)) {
        ProcessBuilder.Execute("cpack");
        ProcessBuilder.Execute("cpush", "-r", string.Format("bud.{0}.nupkg", version));
      }
    }
  }

  public static string GetChocolateySpecDir(IConfig config) {
    return Path.Combine(config.GetDeploymentTemplatesDir(), "ChocolateyPackage");
  }
}