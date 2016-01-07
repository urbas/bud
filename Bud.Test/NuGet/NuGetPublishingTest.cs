using Bud.V1;
using Moq;
using NUnit.Framework;
using static Bud.NuGet.NuGetPublishing;
using static Bud.V1.Api;

namespace Bud.NuGet {
  public class NuGetPublishingTest {
    [Test]
    public void Publishes_the_output_of_a_bare_project() {
      var nuGetPublisher = new Mock<INuGetPublisher>(MockBehavior.Strict);
      var fileToPackage = "Foo.txt";
      nuGetPublisher.Setup(self => self.Publish("Foo",
                                                DefaultVersion,
                                                new[] {fileToPackage}));

      var project = BareProject("fooDir", "Foo")
        .Clear(Output)
        .Add(Output, fileToPackage)
        .Add(NuGetPublishingSupport)
        .SetValue(NuGetPublisher, nuGetPublisher.Object);

      project.Get(Publish);

      nuGetPublisher.VerifyAll();
    }
  }
}