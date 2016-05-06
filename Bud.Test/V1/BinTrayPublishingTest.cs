using System.IO;
using System.Reactive.Linq;
using Bud.IO;
using NUnit.Framework;

namespace Bud.V1 {
  public class BinTrayPublishingTest {
    [Test]
    [Category("IntegrationTest")]
    public void DistributionArchive_contains_FilesToDistribute() {
      using (var tmpDir = new TemporaryDirectory()) {
        var fileA = tmpDir.CreateEmptyFile("A.dll");
        var expectedDistZipPath = Path.Combine(tmpDir.Path, "dist", "A.zip");

        var project = BinTrayPublishing.DistributionSupport
          .Set(BinTrayPublishing.DistributionArchivePath, expectedDistZipPath)
          .Add(BinTrayPublishing.FilesToDistribute, new PackageFile(fileA, "foo/bar/A.dll"));

        var distZipPath = BinTrayPublishing.DistributionArchive[project].Take(1).Wait();

        Assert.AreEqual(expectedDistZipPath, distZipPath);
        ZipTestUtils.IsInZip(distZipPath, "foo/bar/A.dll");
      }
    }
  }
}