using System.Reactive.Linq;
using Bud.IO;
using Bud.V1;
using NUnit.Framework;
using static System.IO.Path;
using static Bud.V1.Api;
using static NUnit.Framework.Assert;

namespace Bud.BaseProjects {
  public class DistributionTest {
    [Test]
    [Category("IntegrationTest")]
    public void DistributionZip_contains_FilesToDistribute() {
      using (var tmpDir = new TemporaryDirectory()) {
        var fileA = tmpDir.CreateEmptyFile("A.dll");
        var expectedDistZipPath = Combine(tmpDir.Path, "dist", "A.zip");

        var project = DistributionSupport
          .SetValue(DistributionArchivePath, expectedDistZipPath)
          .Add(FilesToDistribute, new PackageFile(fileA, "foo/bar/A.dll"));

        var distZipPath = DistributionArchive[project].Take(1).Wait();

        AreEqual(expectedDistZipPath, distZipPath);
        ZipTestUtils.IsInZip(distZipPath, "foo/bar/A.dll");
      }
    }
  }
}