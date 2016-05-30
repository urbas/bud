using NuGet.Frameworks;
using NuGet.Versioning;
using NUnit.Framework;

namespace Bud.NuGet {
  [Category("AppVeyorIgnore")]
  public static class PackageConfigTestUtils {
    public static string CreatePackagesConfigFile(TmpDir tmpDir)
      => CreatePackagesConfigFile(tmpDir, "Bud.NuGet.SinglePackageTest.packages.config");

    public static string CreatePackagesConfigFile(TmpDir tmpDir, string embeddedResourceName)
      => tmpDir.CreateFileFromResource(embeddedResourceName,
                                       tmpDir.Path,
                                       "packages.config");

    public static readonly PackageReference FooReference
      = new PackageReference("Urbas.Example.Foo",
                             NuGetVersion.Parse("1.0.1"),
                             NuGetFramework.Parse("net46"));
  }
}