using System.Reactive.Linq;
using Bud.V1;
using Moq;
using NUnit.Framework;
using static System.IO.Path;
using static Bud.NuGet.NuGetPublishing;
using static Bud.V1.Api;
using static NUnit.Framework.Assert;

namespace Bud.NuGet {
  public class NuGetPublishingTest {
    [Test]
    public void Invokes_the_packager_with_the_right_parameters() {
      var packager = new Mock<IPackager>(MockBehavior.Strict);
      const string package = "Foo.nupkg";
      const string fileToPackage = "Foo.txt";

      var project = BareProject("fooDir", "Foo")
        .Clear(Output)
        .Add(Output, fileToPackage)
        .Add(NuGetPublishingSupport)
        .SetValue(Packager, packager.Object);

      packager.Setup(self => self.Pack(Combine(TargetDir[project], PackageOutputDirName),
                                       "Foo",
                                       DefaultVersion,
                                       new[] {new PackageFile(fileToPackage, "content/Foo.txt")},
                                       new PackageDependency[] {}))
              .Returns(package);

      AreEqual(package, project.Get(Package).Take(1).Wait());
      packager.VerifyAll();
    }
  }
}