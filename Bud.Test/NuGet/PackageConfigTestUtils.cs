using Bud.IO;

namespace Bud.NuGet {
  public static class PackageConfigTestUtils {
    public static string CreatePackagesConfigFile(TemporaryDirectory tmpDir)
      => CreatePackagesConfigFile(tmpDir, "Bud.NuGet.SinglePackageTest.packages.config");

    public static string CreatePackagesConfigFile(TemporaryDirectory tmpDir, string embeddedResourceName)
      => tmpDir.CreateFileFromResource(embeddedResourceName,
                                       tmpDir.Path,
                                       "packages.config");
  }
}