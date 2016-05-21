using System.IO;
using System.Linq;
using Bud.TempDir;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Versioning;
using NUnit.Framework;
using static Bud.NuGet.NuGetPackageDownloader;
using static System.IO.Path;
using static Bud.NuGet.PackageConfigTestUtils;
using static NUnit.Framework.Assert;

namespace Bud.NuGet {
  [Category("IntegrationTest")]
  public class NuGetPackageDownloaderTest {
    private TemporaryDirectory tmpDir;

    [SetUp]
    public void SetUp() => tmpDir = new TemporaryDirectory();

    [TearDown]
    public void TearDown() => tmpDir.Dispose();

    [Test]
    public void CreatePackagesConfigFile_creates_a_valid_packages_config_file() {
      var packagesConfigFile = Combine(tmpDir.Path, "packages.config");
      CreatePackagesConfigFile(new[] {FooReference}, packagesConfigFile);
      using (var fileStream = File.OpenRead(packagesConfigFile)) {
        var packagesConfigReader = new PackagesConfigReader(fileStream);
        AreEqual(new[] {FooReference},
                 packagesConfigReader.GetPackages().Select(ToBudPackageReference));
      }
    }

    [Test]
    public void DownloadPackages_places_the_referenced_packages_into_the_given_folder() {
      IsTrue(InvokeNuGetRestore(new[] {FooReference}, tmpDir.Path));
      That(ReferencedDll(tmpDir), Does.Exist);
    }

    [Test]
    public void DownloadPackages_returns_false_when_failed_to_download_packages() {
      var wrongReferences = new[] {
        new PackageReference("Wrong package ID", NuGetVersion.Parse("9.9.9"), NuGetFramework.UnsupportedFramework)
      };
      IsFalse(InvokeNuGetRestore(wrongReferences, tmpDir.Path));
    }

    private static string ReferencedDll(TemporaryDirectory tmpDir)
      => Combine(tmpDir.Path,
                 "Urbas.Example.Foo.1.0.1/lib/net40/Urbas.Example.Foo.dll");

    private static PackageReference ToBudPackageReference(global::NuGet.Packaging.PackageReference pr)
      => new PackageReference(pr.PackageIdentity.Id, pr.PackageIdentity.Version, pr.TargetFramework);
  }
}