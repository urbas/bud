using System.Collections.Immutable;
using System.Reactive.Linq;
using Bud.NuGet;
using NUnit.Framework;
using static Bud.V1.ChocoPublishing;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  public class ChocoPublishingTest {
    [Test]
    public void Initial_configuration() {
      var packageMetadata = new NuGetPackageMetadata("John Smith", "Foo bar.", ImmutableDictionary<string, string>.Empty);
      var project = Project("A",
                            _ => "MyPackageId",
                            _ => "0.4.2",
                            _ => packageMetadata,
                            _ => Observable.Return("https://foo.bar/blah"),
                            baseDir: "/foo");
      AreEqual("MyPackageId", project.Get(PackageId));
      AreEqual("0.4.2", project.Get(PackageVersion));
      AreEqual(packageMetadata, project.Get(PackageMetadata));
      AreEqual(new[] {"https://foo.bar/blah"}, project.Get(ArchiveUrl).ToEnumerable());
    }

    // TODO: Test integration with NuGet.
  }
}