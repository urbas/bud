using System.IO;
using System.Reactive.Linq;
using Bud.IO;
using Bud.V1;
using NUnit.Framework;

namespace Bud.Dist {
  public class ProjectDistributionTest {
    [Test]
    [Category("IntegrationTest")]
    public void DistributionArchive_contains_FilesToDistribute() {
      using (var tmpDir = new TemporaryDirectory()) {
        var fileA = tmpDir.CreateEmptyFile("A.dll");
        var expectedDistZipPath = Path.Combine(tmpDir.Path, "dist", "A.zip");

        var project = Api.DistributionSupport
          .SetValue(Api.DistributionArchivePath, expectedDistZipPath)
          .Add(Api.FilesToDistribute, new PackageFile(fileA, "foo/bar/A.dll"));

        var distZipPath = Api.DistributionArchive[project].Take(1).Wait();

        Assert.AreEqual(expectedDistZipPath, distZipPath);
        ZipTestUtils.IsInZip(distZipPath, "foo/bar/A.dll");
      }
    }
  }
}