using System.Collections.Generic;
using System.IO;
using static System.IO.Path;
using static Bud.NuGet.NuGetExecutable;

namespace Bud.NuGet {
  public class NuGetPackageDownloader {
    public static bool DownloadPackages(IEnumerable<PackageReference> packageReferences,
                                        string outputDir) {
      var packagesConfigFile = Combine(outputDir, "packages.config");
      CreatePackagesConfigFile(packageReferences, Combine(outputDir, "packages.config"));
      return ExecuteNuGet($"restore {packagesConfigFile} -PackagesDirectory {outputDir}") == 0;
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