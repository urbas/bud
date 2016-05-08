using System.Reactive.Linq;
using Bud.IO;
using NUnit.Framework;
using static System.IO.Path;
using static Bud.V1.Zipping;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  public class ZippingTest {
    [Test]
    public void ZipPath_defaults_to_ProjectId()
      => AreEqual(Combine(ZipProjectA("/foo").Get(Basic.BuildDir), "A.zip"),
                  ZipProjectA("/foo").Get(ZipPath));

    [Test]
    public void Zip_creates_the_zip_archive_at_ZipPath() {
      using (var dir = new TemporaryDirectory()) {
        That(ZipProjectA(dir.Path).Get(Zip).Take(1).Wait(),
             Does.Exist.And.EqualTo(ZipProjectA(dir.Path).Get(ZipPath)));
      }
    }

    [Test]
    public void Zip_archive_contains__files_to_zip() {
      using (var dir = new TemporaryDirectory()) {
        var fileA = dir.CreateFile("foo bar", "A", "foo", "a.txt");
        var project = ZipProjectA(dir.Path)
          .Add(FilesToZip, new PackageFile(fileA, "foo/a.txt"));
        var zip = project.Get(Zip).Take(1).Wait();
        ZipTestUtils.IsInZip(zip, "foo/a.txt", fileA);
      }
    }

    private static Conf ZipProjectA(string baseDir) => ZipProject("A", baseDir: baseDir);
  }
}