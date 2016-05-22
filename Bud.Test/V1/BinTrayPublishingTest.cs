using System.Reactive.Linq;
using NUnit.Framework;
using static Bud.V1.BinTrayPublishing;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  [Category("AppVeyorIgnore")]
  public class BinTrayPublishingTest {
    [Test]
    public void Initial_configuration() {
      var project = Project("A",
                            _ => Observable.Return("/foo/Package.zip"),
                            _ => "MyRepoId",
                            _ => "MyPackageId",
                            _ => "0.4.2",
                            _ => "johnsmith",
                            baseDir: "/foo");
      AreEqual(new[] {"/foo/Package.zip"}, project.Get(PackageFile).ToEnumerable());
      AreEqual("MyRepoId", project.Get(RepositoryId));
      AreEqual("MyPackageId", project.Get(PackageId));
      AreEqual("0.4.2", project.Get(PackageVersion));
      AreEqual("johnsmith", project.Get(Username));
    }

    [Test]
    public void BintrayArchiveDownloadUrl_returns_a_well_formed_Url()
      => AreEqual("https://dl.bintray.com/johnsmith/MyRepoId/MyPackageId-0.4.2.blah",
                  BintrayArchiveDownloadUrl("MyRepoId", "MyPackageId", "johnsmith", "0.4.2", ".blah"));

    [Test]
    public void BintrayPublishPackageUrl_returns_a_well_formed_Url()
      => AreEqual("https://api.bintray.com/content/johnsmith/MyRepoId/MyPackageId/0.4.2/MyPackageId-0.4.2.blah?publish=1",
                  BintrayPublishPackageUrl("MyRepoId", "MyPackageId", "johnsmith", "0.4.2", ".blah"));

    // TODO: Add a test that verifies the actual upload to BinTray.
  }
}