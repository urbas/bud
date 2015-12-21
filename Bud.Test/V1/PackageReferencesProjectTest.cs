using System.IO;
using System.Reactive.Linq;
using Bud.IO;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NUnit.Framework;
using static Bud.V1.Api;

namespace Bud.V1 {
  public class PackageReferencesProjectTest {
    [Test]
    public void Output_is_empty_when_packages_config_is_missing()
      => Assert.AreEqual(InOut.Empty,
                         Output[TestProject("foo")].Take(1).Wait());

    [Test]
    public void Packages_config_file_is_at_the_root_by_default()
      => Assert.AreEqual(Path.Combine("foo", "packages.config"),
                         PackagesConfigPath[TestProject("foo")]);

    [Test]
    public void Input_contains_the_packages_config_file()
      => Assert.AreEqual(new InOut(Path.Combine("foo", "packages.config")),
                         Input[TestProject("foo")].Take(1).Wait());

    [Test]
    public void PackageReferences_lists_entries_specified_in_the_packages_config_file() {
      using (var tmpDir = new TemporaryDirectory()) {
        AddPackagesConfigFile(tmpDir);
        var project = TestProject(tmpDir.Path);
        Assert.That(PackageReferences[project],
                    Is.EqualTo(new[] {new PackageReference(new PackageIdentity("Urbas.Example.Foo", NuGetVersion.Parse("1.0.1")), NuGetFramework.Parse("net46"))})
                      .Using(new PackageReferenceComparer()));
      }
    }

    private static Conf TestProject(string path)
      => PackageReferencesProject(path, "Foo.References");

    private static string AddPackagesConfigFile(TemporaryDirectory tmpDir)
      => tmpDir.CreateFileFromResource("Bud.NuGet.SinglePackageTest.packages.config",
                                       "packages.config");
  }
}