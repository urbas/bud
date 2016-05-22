using NuGet.Frameworks;
using NuGet.Versioning;
using NUnit.Framework;
using static Bud.IO.MemoryStreams;

namespace Bud.NuGet {
  [Category("IntegrationTest")]
  [Category("AppVeyorIgnore")]
  public class NuGetPackageReferencesReaderTest {
    [Test]
    public void Load_returns_a_list_of_package_references() {
      var configStream = ToMemoryStream("<?xml version=\"1.0\" encoding=\"utf-8\"?><packages><package id=\"A\" version=\"1.2.3\" targetFramework=\"net45\" /></packages>");
      Assert.AreEqual(new[] {new PackageReference("A", NuGetVersion.Parse("1.2.3"), NuGetFramework.Parse("net45"))},
                      NuGetPackageReferencesReader.LoadReferences(configStream));
    }

    [Test]
    public void Load_returns_a_list_of_package_references_with_no_package_version_or_framework_version() {
      var configStream = ToMemoryStream("<?xml version=\"1.0\" encoding=\"utf-8\"?><packages><package id=\"A\" version=\"1.2.3\" /></packages>");
      Assert.AreEqual(new[] {new PackageReference("A", NuGetVersion.Parse("1.2.3"), NuGetFramework.UnsupportedFramework)},
                      NuGetPackageReferencesReader.LoadReferences(configStream));
    }
  }
}