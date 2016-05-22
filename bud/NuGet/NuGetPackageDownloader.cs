using System.Collections.Generic;
using System.IO;
using Bud.Cli;
using static System.IO.Path;

namespace Bud.NuGet {
  public class NuGetPackageDownloader {
    public virtual bool DownloadPackages(IEnumerable<PackageReference> packageReference, string packagesCacheDir)
      => InvokeNuGetRestore(packageReference, packagesCacheDir);

    public static bool InvokeNuGetRestore(IEnumerable<PackageReference> packageReferences,
                                          string outputDir) {
      var packagesConfigFile = Combine(outputDir, "packages.config");
      CreatePackagesConfigFile(packageReferences, Combine(outputDir, "packages.config"));
      return BatchExec.Run("nuget", $"restore {packagesConfigFile} -PackagesDirectory {outputDir}") == 0;
    }

    public static void CreatePackagesConfigFile(IEnumerable<PackageReference> packageReferences,
                                                string packagesConfigFile) {
      using (var outputStream = File.Create(packagesConfigFile)) {
        using (var writer = new StreamWriter(outputStream)) {
          PackageReference.WritePackagesConfigXml(packageReferences, writer);
        }
      }
    }
  }
}