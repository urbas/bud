using Bud.IO;
using Bud.TempDir;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace Bud.NuGet {
  public static class PackageConfigTestUtils {
    public static string CreatePackagesConfigFile(TemporaryDirectory tmpDir)
      => CreatePackagesConfigFile(tmpDir, "Bud.NuGet.SinglePackageTest.packages.config");

    public static string CreatePackagesConfigFile(TemporaryDirectory tmpDir, string embeddedResourceName)
      => tmpDir.CreateFileFromResource(embeddedResourceName,
                                       tmpDir.Path,
                                       "packages.config");

    public static readonly PackageReference FooReference
      = new PackageReference("Urbas.Example.Foo",
                             NuGetVersion.Parse("1.0.1"),
                             NuGetFramework.Parse("net46"));
  }
}