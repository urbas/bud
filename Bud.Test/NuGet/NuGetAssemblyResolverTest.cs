using System.Linq;
using Bud.IO;
using NUnit.Framework;
using static System.IO.Path;
using static Bud.NuGet.PackageConfigTestUtils;
using static NUnit.Framework.Assert;

namespace Bud.NuGet {
  [Category("IntegrationTest")]
  public class NuGetPackageResolverTest {
    [Test]
    public void Resolve_a_list_of_assemblies_referenced_in_the_package() {
      using (var tmpDir = new TemporaryDirectory()) {
        var packagesConfigFile = CreatePackagesConfigFile(tmpDir);

        var assemblies = new NuGetPackageResolver()
          .Resolve(new[] {packagesConfigFile}, tmpDir.Path);

        That(assemblies.Select(GetFullPath),
             Has.Member(GetFullPath(ReferencedDll(tmpDir))));
      }
    }

    [Test]
    public void Resolve_returns_a_list_of_existing_assembly_dlls() {
      using (var tmpDir = new TemporaryDirectory()) {
        var packagesConfigFile = CreatePackagesConfigFile(tmpDir);

        var assemblies = new NuGetPackageResolver()
          .Resolve(new[] {packagesConfigFile}, tmpDir.Path);

        That(assemblies, Has.All.Exist);
      }
    }

    private static string ReferencedDll(TemporaryDirectory tmpDir)
      => Combine(tmpDir.Path,
                 "packages/Urbas.Example.Foo.1.0.1/lib/net40/Urbas.Example.Foo.dll");
  }
}