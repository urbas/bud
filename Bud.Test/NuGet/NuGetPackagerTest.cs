using System.Collections.Generic;
using System.Collections.Immutable;
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
    private IPackager packager;
    private const string Authors = "Zar McFoobar";
    private const string Description = "A test project.";

    [SetUp]
    public void SetUp() => packager = new NuGetPackager();

    [Test]
    public void Creates_a_package() {
      using (var tmpDir = new TemporaryDirectory()) {
        var txtFile = tmpDir.CreateEmptyFile("A.txt");
        var dllFile = tmpDir.CreateEmptyFile("Foo.Bar.dll");
        var optionalFields = new Dictionary<string, string> {
          {"title", "foo-title"},
          {"owners", "foo-owners"},
          {"releaseNotes", "foo-releaseNotes"},
          {"summary", "foo-summary"},
          {"language", "foo-language"},
          {"projectUrl", "http://example.com/project"},
          {"iconUrl", "http://example.com/icon"},
          {"licenseUrl", "http://example.com/license"},
          {"copyright", "foo-copyright"},
          {"requireLicenseAcceptance", "true"},
          {"tags", "foo-tags"},
          {"developmentDependency", "true"}
        }.ToImmutableDictionary();

        var package = Pack(tmpDir, txtFile, dllFile, optionalFields);

        using (var fileStream = OpenRead(package)) {
          var packageReader = new PackageReader(fileStream);
          var nuspecReader = new NuspecReader(packageReader.GetNuspec());
          var metadata = nuspecReader.GetMetadata().ToDictionary(pair => pair.Key, pair => pair.Value);
          AssertLibFilePresent(packageReader);
          AssertContentGroupPresent(packageReader);
          AssertMetadataMatches(optionalFields, metadata);
        }
        That(package, Does.Exist);
      }
    }

    private string Pack(TemporaryDirectory tmpDir, string txtFile, string dllFile, IImmutableDictionary<string, string> optionalFields)
      => packager.Pack(tmpDir.CreateDir("out"),
                       "Foo.Bar",
                       "1.2.3",
                       new[] {
                         new PackageFile(txtFile, "content/A.txt"),
                         new PackageFile(dllFile, "lib/net40/Foo.Bar.dll"),
                       },
                       new[] {
                         new PackageDependency("Moo.Zar", "3.2.1")
                       },
                       new NuGetPackageMetadata(Authors,
                                                Description,
                                                optionalFields));

    private static void AssertMetadataMatches(ImmutableDictionary<string, string> optionalFields, Dictionary<string, string> metadata) {
      var allFields = optionalFields.Add("authors", Authors)
                                    .Add("version", "1.2.3")
                                    .Add("description", Description)
                                    .Add("id", "Foo.Bar");
      That(metadata, Is.EquivalentTo(allFields));
    }

    private static void AssertLibFilePresent(PackageReader packageReader) {
      var libGroup = packageReader.GetLibItems().Single();
      AreEqual(NuGetFramework.Parse("net40"),
               libGroup.TargetFramework);
      AreEqual(new[] {"lib/net40/Foo.Bar.dll"},
               libGroup.Items);
    }

    private static void AssertContentGroupPresent(PackageReader packageReader) {
      var contentGroup = packageReader.GetContentItems().Single();
      AreEqual(NuGetFramework.AnyFramework,
               contentGroup.TargetFramework);
      AreEqual(new[] {"content/A.txt"},
               contentGroup.Items);
    }
  }
}