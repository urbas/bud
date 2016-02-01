using System.Linq;
using Bud.IO;
using NuGet.Frameworks;
using NuGet.Versioning;
using NUnit.Framework;
using static System.IO.Path;
using static Bud.NuGet.NuGetPackageDownloader;
using static NUnit.Framework.Assert;

namespace Bud.NuGet {
  [Category("IntegrationTest")]
  public class NuGetPackageResolverTest {
    private TemporaryDirectory tmpDir;
    private string packagesDir;

    private readonly PackageReference packageReference
      = new PackageReference("Urbas.Example.Foo",
                             NuGetVersion.Parse("1.0.1"),
                             NuGetFramework.Parse("net40"));

    [SetUp]
    public void SetUp() {
      tmpDir = new TemporaryDirectory();
      packagesDir = tmpDir.CreateDir("packages");
      DownloadPackages(new[] {packageReference}, packagesDir);
    }

    [TearDown]
    public void TearDown() => tmpDir.Dispose();

    [Test]
    public void Resolve_a_list_of_assemblies_referenced_in_the_package() {
      var assemblies = new NuGetPackageResolver()
        .Resolve(new[] {packageReference}, packagesDir, tmpDir.Path);

      That(assemblies.Select(GetFullPath),
           Has.Member(GetFullPath(ReferencedDll(tmpDir))));
    }

    [Test]
    public void Resolve_returns_a_list_of_existing_assembly_dlls() {
      var assemblies = new NuGetPackageResolver()
        .Resolve(new[] {packageReference}, packagesDir, tmpDir.Path);

      That(assemblies, Has.All.Exist);
    }

    private static string ReferencedDll(TemporaryDirectory tmpDir)
      => Combine(tmpDir.Path,
                 "packages/Urbas.Example.Foo.1.0.1/lib/net40/Urbas.Example.Foo.dll");
  }
}