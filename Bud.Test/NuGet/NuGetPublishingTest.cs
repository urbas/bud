using System;
using System.Collections.Immutable;
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

      packager.Setup(self => self.Pack(Combine(BudDir[project], PackageOutputDirName),
                                       "Foo",
                                       DefaultVersion,
                                       new[] {new PackageFile(fileToPackage, "content/Foo.txt")},
                                       new PackageDependency[] {},
                                       new NuGetPackageMetadata(Environment.UserName, "Foo", ImmutableDictionary<string, string>.Empty)))
              .Returns(package);

      AreEqual(package, project.Get(Package).Take(1).Wait());
      packager.VerifyAll();
    }

    [Test]
    public void Invokes_the_publisher_with_the_right_parameters() {
      var packager = new Mock<IPublisher>(MockBehavior.Strict);
      const string package = "Foo.nupkg";

      var project = BareProject("fooDir", "Foo")
        .Add(NuGetPublishingSupport)
        .SetValue(Publisher, packager.Object)
        .SetValue(Package, Observable.Return(package))
        .SetValue(PublishUrl, "publish url")
        .SetValue(PublishApiKey, "publish api key");

      packager.Setup(self => self.Publish(package,
                                       "publish url",
                                       "publish api key"))
              .Returns(true);

      AreEqual(new [] {true}, project.Get(Publish).ToEnumerable());
      packager.VerifyAll();
    }
  }
}