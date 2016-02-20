using System;
using System.IO;
using System.Linq;
using Bud.Cli;
using Bud.IO;
using Bud.NuGet;

namespace Bud.Distribution {
  internal static class ChocoDistribution {
    public static bool PushToChoco(string repositoryId, string packageId, string packageVersion, string archiveUrl, string username, string buildDir, NuGetPackageMetadata packageMetadata) {
      var scratchDir = CreateChocoScratchDir(buildDir);
      var installScriptPath = CreateChocoInstallScript(packageId, archiveUrl, scratchDir);
      var distPackage = CreateChocoPackage(packageId, packageVersion, username, scratchDir, installScriptPath, packageMetadata);
      Console.WriteLine($"Starting to push to chocolatey ...");
      var success = Exec.Run("cpush", distPackage) == 0;
      Console.WriteLine($"Push to chocolatey success: {success}");
      return success;
    }

    private static string CreateChocoPackage(string packageId, string packageVersion, string username, string scratchDir, string installScriptPath, NuGetPackageMetadata packageMetadata) {
      return NuGetPackager.CreatePackage(
        scratchDir,
        Directory.GetCurrentDirectory(),
        packageId,
        packageVersion,
        new[] {new PackageFile(installScriptPath, "tools/chocolateyInstall.ps1"),},
        Enumerable.Empty<PackageDependency>(),
        packageMetadata,
        "-NoPackageAnalysis");
    }

    private static string CreateChocoScratchDir(string buildDir) {
      var chocoDistDir = Path.Combine(buildDir, "choco-dist-package");
      Directory.CreateDirectory(chocoDistDir);
      return chocoDistDir;
    }

    private static string CreateChocoInstallScript(string packageId, string archiveUrl, string scratchDir) {
      var chocoInstallScriptPath = Path.Combine(scratchDir, "chocolateyInstall.ps1");
      var chocolateyInstallScript = ChocolateyInstallScript(packageId, archiveUrl);
      File.WriteAllText(chocoInstallScriptPath, chocolateyInstallScript);
      return chocoInstallScriptPath;
    }

    private static string ChocolateyInstallScript(string packageId,
                                                  string archiveUrl)
      => $"Install-ChocolateyZipPackage '{packageId}' " +
         $"'{archiveUrl}' " +
         "\"$(Split-Path -parent $MyInvocation.MyCommand.Definition)\"";
  }
}