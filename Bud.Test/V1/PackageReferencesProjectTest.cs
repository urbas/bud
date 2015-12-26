using System.IO;
using System.Reactive.Linq;
using Bud.IO;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NUnit.Framework;
using static Bud.V1.Api;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  public class PackageReferencesProjectTest {
    [Test]
    public void Packages_config_file_is_at_the_root_by_default()
      => That(PackagesConfigFile[TestProject()],
              Is.EqualTo(Path.Combine("foo", "packages.config")));

    [Test]
    public void PackageReferences_is_initially_empty()
      => That(PackageReferences[TestProject()].Take(1).ToEnumerable(),
              Has.Exactly(1).Empty);

    [Test]
    public void PackageReferences_lists_contents_of_the_packages_config_file() {
      using (var tmpDir = new TemporaryDirectory()) {
        CreatePackagesConfigFile(tmpDir);
        That(PackageReferences[TestProject(tmpDir.Path)].Take(1).ToEnumerable(),
             Has.Exactly(1)
                .EqualTo(new[] {new PackageReference(new PackageIdentity("Urbas.Example.Foo", NuGetVersion.Parse("1.0.1")), NuGetFramework.Parse("net46"))})
                .Using(new PackageReferenceComparer()));
      }
    }

    private static string CreatePackagesConfigFile(TemporaryDirectory tmpDir)
      => tmpDir.CreateFileFromResource("Bud.NuGet.SinglePackageTest.packages.config",
                                       tmpDir.Path,
                                       "packages.config");

    private static Conf TestProject(string baseDir = "foo")
      => PackageReferencesProject(baseDir, "Foo.References");
  }
}