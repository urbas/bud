using System.Reactive.Linq;
using Bud.V1;
using Moq;
using NUnit.Framework;
using static Bud.NuGet.NuGetPublishing;
using static Bud.V1.Api;

namespace Bud.NuGet {
  public class NuGetPublishingTest {
    [Test]
    public void Publishes_the_output_of_a_bare_project() {
      var nuGetPublisher = new Mock<IPublisher>(MockBehavior.Strict);
      const string fileToPackage = "Foo.txt";
      nuGetPublisher.Setup(self => self.Publish("Foo",
                                                DefaultVersion,
                                                new[] {new PackageFile(fileToPackage, "content/Foo.txt")},
                                                new PackageDependency[] {}));

      var project = BareProject("fooDir", "Foo")
        .Clear(Output)
        .Add(Output, fileToPackage)
        .Add(NuGetPublishingSupport)
        .SetValue(Publisher, nuGetPublisher.Object);

      project.Get(Publish).Take(1).Wait();

      nuGetPublisher.VerifyAll();
    }
  }
}