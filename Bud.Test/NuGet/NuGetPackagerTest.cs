using System.Linq;
using Bud.IO;
using NuGet.Frameworks;
using NuGet.Packaging;
using NUnit.Framework;
using static System.IO.File;
using static NUnit.Framework.Assert;

namespace Bud.NuGet {
  [Category("IntegrationTest")]
  public class NuGetPackagerTest {
    [Test]
    public void Creates_a_package() {
      using (var tmpDir = new TemporaryDirectory()) {
        IPackager packager = new NuGetPackager();
        var outputDirectory = tmpDir.CreateDir("out");
        var txtFile = tmpDir.CreateEmptyFile("A.txt");
        var dllFile = tmpDir.CreateEmptyFile("Foo.Bar.dll");
        var package = packager.Pack(outputDirectory,
                                    "Foo.Bar",
                                    "1.2.3",
                                    new[] {
                                      new PackageFile(txtFile, "content/A.txt"),
                                      new PackageFile(dllFile, "lib/net40/Foo.Bar.dll"),
                                    },
                                    new[] {
                                      new PackageDependency("Moo.Zar", "3.2.1")
                                    });

        using (var fileStream = OpenRead(package)) {
          var packageReader = new PackageReader(fileStream);
          var libGroup = packageReader.GetLibItems().Single();
          AreEqual(NuGetFramework.Parse("net40"),
                          libGroup.TargetFramework);
          AreEqual(new[] {"lib/net40/Foo.Bar.dll"},
                          libGroup.Items);
          var contentGroup = packageReader.GetContentItems().Single();
          AreEqual(NuGetFramework.AnyFramework,
                          contentGroup.TargetFramework);
          AreEqual(new[] {"content/A.txt"},
                          contentGroup.Items);
        }
        That(package, Does.Exist);
      }
    }
  }
}