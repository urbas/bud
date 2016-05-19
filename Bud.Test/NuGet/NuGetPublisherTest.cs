using System.IO;
using Bud.IO;
using Moq;
using NUnit.Framework;
using static Bud.Util.Option;
using static NUnit.Framework.Assert;

namespace Bud.NuGet {
  [Category("IntegrationTest")]
  public class NuGetPublisherTest {
    private const string OutputPackageName = "Foo.Bar.1.2.3.nupkg";
    private IPublisher publisher;

    [SetUp]
    public void SetUp() => publisher = new NuGetPublisher();

    [Test]
    public void Publish_places_the_package_into_the_source_folder() {
      using (var tmpDir = new TemporaryDirectory()) {
        var package = tmpDir.CreateFileFromResource($"Bud.NuGet.{OutputPackageName}",
                                                    OutputPackageName);
        var repository = tmpDir.CreateDir("repository");
        IsTrue(publisher.Publish(package, $"file://{repository}", None<string>()));
        That(Path.Combine(repository, OutputPackageName),
             Does.Exist);
      }
    }

    [Test]
    public void Publish_returns_false_on_failure() {
      using (var tmpDir = new TemporaryDirectory()) {
        var package = tmpDir.CreatePath(OutputPackageName);
        var repository = tmpDir.CreateDir("repository");
        IsFalse(publisher.Publish(package, $"file://{repository}", None<string>()));
        That(Path.Combine(repository, OutputPackageName),
             Does.Not.Exist);
      }
    }

    [Test]
    public void Publish_uses_command_line_builder_and_executor() {
      var cliArgsBuilder = new Mock<NuGetPushArgsBuilder>(MockBehavior.Strict);
      cliArgsBuilder.Setup(s => s.CreateArgs("nuget-package", "targetUrl", "apiKey"))
                    .Returns("Foo");
      var nuGetExecutable = new Mock<NuGetExecutable>(MockBehavior.Strict);
      nuGetExecutable.Setup(s => s.Run("Foo"))
                     .Returns(true);
      publisher = new NuGetPublisher(nuGetExecutable.Object, cliArgsBuilder.Object);
      IsTrue(publisher.Publish("nuget-package", "targetUrl", "apiKey"));
      cliArgsBuilder.VerifyAll();
      nuGetExecutable.VerifyAll();
    }
  }
}