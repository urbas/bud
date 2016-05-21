using System;
using System.Collections.Immutable;
using System.IO;
using System.Reactive.Linq;
using Bud.IO;
using Bud.NuGet;
using Bud.TempDir;
using Bud.Util;
using Moq;
using NuGet.Frameworks;
using NuGet.Versioning;
using NUnit.Framework;
using static Bud.V1.Basic;
using static Bud.V1.Builds;
using static Bud.V1.NuGetPublishing;

namespace Bud.V1 {
  public class NuGetPublishingTest {
    [Test]
    public void Invokes_the_packager_with_the_right_parameters() {
      var packager = new Mock<IPackager>(MockBehavior.Strict);
      const string package = "B.nupkg";
      const string fileToPackage = "B.txt";

      var projectA = Project("A", baseDir: "/foo")
        .Set(ProjectVersion, "1.2.3");
      var project = Project("B", baseDir: "/foo")
        .Clear(Output).Add(Output, fileToPackage)
        .Add(Dependencies, "../A")
        .InitEmpty(NuGetReferences.ReferencedPackages)
        .Add(NuGetReferences.ReferencedPackages, new PackageReference("C", NuGetVersion.Parse("4.5.7"), NuGetFramework.Parse("net35")))
        .Add(NuGetPublishingConf)
        .Set(Packager, packager.Object);
      var projects = Projects(projectA, project);

      packager.Setup(self => self.Pack(Path.Combine(BuildDir[project], PackageOutputDirName),
                                       "/foo",
                                       "B",
                                       DefaultVersion,
                                       new[] {new PackageFile(fileToPackage, "content/B.txt")},
                                       new[] {new PackageDependency("A", "1.2.3"), new PackageDependency("C", "4.5.7")},
                                       new NuGetPackageMetadata(Environment.UserName, "B", ImmutableDictionary<string, string>.Empty)))
              .Returns(package);

      Assert.AreEqual(package, projects.Get("B"/Package).Take(1).Wait());
      packager.VerifyAll();
    }

    [Test]
    [Category("IntegrationTest")]
    public void Simplest_NuGet_publishing_project() {
      using (var dir = new TemporaryDirectory()) {
        var project = NuGetPublishingProject("Foo", baseDir: dir.Path)
          .Set(PublishUrl, dir.CreateDir("repo"));
        Assert.AreEqual(new[] {false},
                 Publish[project].ToEnumerable());
      }
    }

    [Test]
    [Category("IntegrationTest")]
    public void NuGet_publishing_some_content() {
      using (var dir = new TemporaryDirectory()) {
        var repoDir = dir.CreateDir("repo");
        var someFile = dir.CreateFile("some content", "A.txt");
        var project = NuGetPublishingProject("A", baseDir: dir.Path)
          .Add(PackageFiles, new PackageFile(someFile, "content/A.txt"))
          .Set(PublishUrl, repoDir);
        Assert.AreEqual(new[] {true},
                 Publish[project].ToEnumerable());
      }
    }

    [Test]
    public void Invokes_the_publisher_with_the_right_parameters() {
      var packager = new Mock<IPublisher>(MockBehavior.Strict);
      const string package = "Foo.nupkg";

      var project = Project("A", baseDir: "/foo")
        .Add(NuGetPublishingConf)
        .Set(Publisher, packager.Object)
        .Set(Package, Observable.Return(package))
        .Set(PublishUrl, "publish url")
        .Set(PublishApiKey, "publish api key");

      packager.Setup(self => self.Publish(package,
                                          "publish url",
                                          "publish api key"))
              .Returns(true);

      Assert.AreEqual(new[] {true}, project.Get(Publish).ToEnumerable());
      packager.VerifyAll();
    }

    [Test]
    public void Default_packager_is_the_NuGet_CLI_implementation()
      => Assert.That(Project("A", baseDir: "/foo")
                .Add(NuGetPublishingConf)
                .Get(Packager),
              Is.InstanceOf<NuGetPackager>());

    [Test]
    public void Default_publisher_is_the_NuGet_CLI_implementation()
      => Assert.That(Project("A", baseDir: "/foo")
                .Add(NuGetPublishingConf)
                .Get(Publisher),
              Is.InstanceOf<NuGetPublisher>());

    [Test]
    public void Default_PublishApiKey_is_set_to_None()
      => Assert.That(Project("A", baseDir: "/foo")
                .Add(NuGetPublishingConf)
                .Get(PublishApiKey),
              Is.EqualTo(Option.None<string>()));

    [Test]
    public void Default_PackageMetadata_has_no_ProjectUrl()
      => Assert.That(Project("A", baseDir: "/foo")
                .Add(NuGetPublishingConf)
                .Get(PackageMetadata).OptionalFields,
              Does.Not.Contain("projectUrl"));

    [Test]
    public void Default_PackageMetadata_has_the_provided_ProjectUrl()
      => Assert.That(Project("A", baseDir: "/foo")
                .Add(NuGetPublishingConf)
                .Set(ProjectUrl, "some url")
                .Get(PackageMetadata).OptionalFields["projectUrl"],
              Is.EqualTo("some url"));
  }
}