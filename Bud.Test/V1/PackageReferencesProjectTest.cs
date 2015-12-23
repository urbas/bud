using System.IO;
using NUnit.Framework;
using static Bud.V1.Api;

namespace Bud.V1 {
  public class PackageReferencesProjectTest {
    [Test]
    public void Packages_config_file_is_at_the_root_by_default()
      => Assert.AreEqual(Path.Combine("foo", "packages.config"),
                         PackagesConfigFile[TestProject()]);

    private static Conf TestProject()
      => PackageReferencesProject("foo", "Foo.References");
  }
}