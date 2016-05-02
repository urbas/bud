using System.IO;
using Bud.IO;
using NUnit.Framework;

namespace Bud.Dist {
  public class ZipperTest {
    [Test]
    [Category("IntegrationTest")]
    public void DistributionArchive_contains_FilesToDistribute() {
      using (var tmpDir = new TemporaryDirectory()) {
        var fileA = tmpDir.CreateEmptyFile("A.dll");
        var expectedDistZipPath = Path.Combine(tmpDir.Path, "dist", "A.zip");
        Zipper.CreateZip(expectedDistZipPath, new []{ new PackageFile(fileA, "foo/bar/A.dll") });
        ZipTestUtils.IsInZip(expectedDistZipPath, "foo/bar/A.dll");
      }
    }
  }
}