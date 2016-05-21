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
        tmpDir.CreateEmptyFile("A.txt");
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

        var package = Pack(tmpDir, optionalFields, new[] {
          new PackageFile("A.txt", "content/A.txt"),
          new PackageFile(dllFile, "lib/net40/Foo.Bar.dll"),
        });

        using (var fileStream = OpenRead(package)) {
          var packageReader = new PackageArchiveReader(fileStream);
          var nuspecReader = new NuspecReader(packageReader.GetNuspec());
          var metadata = nuspecReader.GetMetadata().ToDictionary(pair => pair.Key, pair => pair.Value);
          AssertLibFilePresent(packageReader);
          AssertContentGroupPresent(packageReader);
          AssertMetadataMatches(optionalFields, metadata);
        }
        That(package, Does.Exist);
      }
    }

    [Test]
    public void Package_does_not_contain_files_from_a_previous_packaging() {
      using (var tmpDir = new TemporaryDirectory()) {
        var txtFile = tmpDir.CreateEmptyFile("A.txt");
        var dllFile = tmpDir.CreateEmptyFile("Foo.Bar.dll");
        var optionalFields = ImmutableDictionary<string, string>.Empty;

        Pack(tmpDir, optionalFields, new[] {
          new PackageFile(txtFile, "content/A.txt"),
          new PackageFile(dllFile, "lib/net40/Foo.Bar.dll"),
        });
        var package = Pack(tmpDir, optionalFields, new[] {
          new PackageFile(txtFile, "content/A.txt"),
        });

        using (var fileStream = OpenRead(package)) {
          var packageReader = new PackageArchiveReader(fileStream);
          IsEmpty(packageReader.GetLibItems());
        }
        That(package, Does.Exist);
      }
    }

    [Test]
    [Ignore("There's a bug in NuGet triggered by directories that are prefixed with a dot.")]
    public void Package_can_be_created_with_files_in_dot_prefixed_directories() {
      using (var tmpDir = new TemporaryDirectory()) {
        var dllFile = tmpDir.CreateEmptyFile(".Foo", "A.dll");

        var package = Pack(tmpDir,
                           ImmutableDictionary<string, string>.Empty,
                           new[] {new PackageFile(dllFile, "lib/A.dll")},
                           new PackageDependency[] {});
        
        That(package, Does.Exist);
      }
    }

    private string Pack(TemporaryDirectory baseDir, IImmutableDictionary<string, string> optionalFields, PackageFile[] packageFiles) {
      var packageDependencies = new[] {
        new PackageDependency("Moo.Zar", "3.2.1")
      };
      return Pack(baseDir, optionalFields, packageFiles, packageDependencies);
    }

    private string Pack(TemporaryDirectory baseDir, IImmutableDictionary<string, string> optionalFields, PackageFile[] packageFiles, PackageDependency[] packageDependencies)
      => packager.Pack(baseDir.CreateDir("out"),
                       baseDir.Path,
                       "Foo.Bar",
                       "1.2.3",
                       packageFiles,
                       packageDependencies,
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

    private static void AssertLibFilePresent(PackageArchiveReader packageReader) {
      var libGroup = packageReader.GetLibItems().Single();
      AreEqual(NuGetFramework.Parse("net40"),
               libGroup.TargetFramework);
      AreEqual(new[] {"lib/net40/Foo.Bar.dll"},
               libGroup.Items);
    }

    private static void AssertContentGroupPresent(PackageArchiveReader packageReader) {
      var contentGroup = packageReader.GetContentItems().Single();
      AreEqual(NuGetFramework.AnyFramework,
               contentGroup.TargetFramework);
      AreEqual(new[] {"content/A.txt"},
               contentGroup.Items);
    }
  }
}