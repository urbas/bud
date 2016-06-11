using System.IO;
using NuGet.Frameworks;
using NuGet.Versioning;
using NUnit.Framework;

namespace Bud.NuGet {
  public class PackageReferenceTest {
    private static readonly PackageReference FooPackageReference = new PackageReference("Foo", NuGetVersion.Parse("1.2.3"), NuGetFramework.Parse("net45"));
    private static readonly PackageReference BarPackageReference = new PackageReference("Bar", NuGetVersion.Parse("1.2.3"), NuGetFramework.Parse("net45"));

    [Test]
    public void Package_references_with_same_id_version_and_framework_are_equal()
      => Assert.AreEqual(FooPackageReference, FooPackageReference);

    [Test]
    public void Package_references_with_different_id_version_and_framework_are_equal()
      => Assert.AreNotEqual(FooPackageReference, BarPackageReference);

    [Test]
    public void Package_references_equal_self()
      => Assert.IsTrue(FooPackageReference.Equals(FooPackageReference));

    [Test]
    public void Package_references_are_not_equal_to_null()
      => Assert.IsFalse(FooPackageReference.Equals(null));

    [Test]
    public void Package_that_equal_have_equal_hashes()
      => Assert.AreEqual(FooPackageReference.GetHashCode(),
                         FooPackageReference.GetHashCode());

    [Test]
    public void WritePackagesConfigXml_contains_listed_packages() {
      using (var textWriter = new StringWriter()) {
        PackageReference.WritePackagesConfigXml(new[] {FooPackageReference},
                                                textWriter);
        Assert.That(textWriter.ToString(),
                    Does.Contain(@"<package id=""Foo"" version=""1.2.3"" targetFramework=""net45"" />"));
      }
    }
  }
}