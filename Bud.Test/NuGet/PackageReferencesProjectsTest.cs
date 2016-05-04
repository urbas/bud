using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Bud.V1;
using Moq;
using NUnit.Framework;
using static System.IO.File;
using static System.IO.Path;
using static Bud.NuGet.PackageConfigTestUtils;
using static Bud.V1.Api;
using static NUnit.Framework.Assert;

namespace Bud.NuGet {
  public class PackageReferencesProjectsTest {
    [Test]
    public void Packages_config_file_is_in_ProjectDir_by_default()
      => That(PackageReferencesProject("A", baseDir: "/foo").Get(PackagesConfigFile),
              Is.EqualTo(Combine("/foo", "A", "packages.config")));

    [Test]
    public void Assemblies_is_initially_empty()
      => That(PackageReferencesProject("A", baseDir: "fooDir")
                .Get(ResolvedAssemblies)
                .Take(1).ToEnumerable(),
              Has.Exactly(1).Empty);

    [Test]
    [Category("IntegrationTest")]
    public void PackageDownloader_puts_assemblies_into_the_target_folder() {
      using (var dir = new TemporaryDirectory()) {
        CreatePackagesConfigFile(dir);
        var resolvedAssemblies = ImmutableList.Create("Foo.dll", "Bar.dll");
        var resolver = MockResolver(new[] {FooReference}, resolvedAssemblies, dir);
        var project = PackageReferencesProject("A", ".", dir.Path)
          .Set(AssemblyResolver, resolver.Object)
          .ToCompiled();

        ("A"/ResolvedAssemblies)[project].Take(1).Wait();

        That(ReadResolvedAssembliesCache(project),
             Is.EquivalentTo(resolvedAssemblies));
      }
    }

    [Test]
    [Category("IntegrationTest")]
    public void PackageDownloader_is_not_invoked_when_given_no_package_references() {
      using (var tmpDir = new TemporaryDirectory()) {
        var downloader = new Mock<NuGetPackageDownloader>(MockBehavior.Strict);
        var project = PackageReferencesProject("A", baseDir: tmpDir.Path)
          .Clear(ReferencedPackages)
          .Set(PackageDownloader, downloader.Object);

        ResolvedAssemblies[project].Take(1).Wait();
      }
    }

    [Test]
    public void PackageDownloader_and_AssemblyResolver_are_invoked_with_a_list_of_package_references() {
      using (var dir = new TemporaryDirectory()) {
        CreatePackagesConfigFile(dir);
        var expectedAssemblies = ImmutableList.Create("Foo.dll");
        var downloader = MockDownloader(new[] {FooReference}, dir);
        var resolver = MockResolver(new[] {FooReference}, expectedAssemblies, dir);
        var project = PackageReferencesProject("A", ".", dir.Path)
          .Set(PackageDownloader, downloader.Object)
          .Set(AssemblyResolver, resolver.Object);

        var actualAssemblies = ResolvedAssemblies[project].Take(1).ToEnumerable();

        That(actualAssemblies, Has.Exactly(1).EqualTo(expectedAssemblies));
        resolver.VerifyAll();
        downloader.VerifyAll();
      }
    }

    [Test]
    public void Assemblies_are_loaded_from_cache() {
      using (var dir = new TemporaryDirectory()) {
        CreatePackagesConfigFile(dir);
        var resolver = new Mock<IAssemblyResolver>(MockBehavior.Strict);
        var project = PackageReferencesProject("A", ".", dir.Path)
          .Set(AssemblyResolver, resolver.Object)
          .ToCompiled();
        dir.CreateFile(
          "4D-31-2B-41-83-A6-87-D8-FC-8C-92-C7-F3-CE-60-E9\nMoo.dll\nZoo.dll",
          project.Get("A"/Basic.BuildDir), "resolved_assemblies");

        ("A"/ResolvedAssemblies)[project].Take(1).Wait();

        That(ReadResolvedAssembliesCache(project),
             Is.EquivalentTo(new[] {"Moo.dll", "Zoo.dll"}));
      }
    }

    [Test]
    public void ReferencedPackages_lists_package_references_read_from() {
      using (var dir = new TemporaryDirectory()) {
        CreatePackagesConfigFile(dir);
        var project = PackageReferencesProject("A", ".", dir.Path);
        That(project.Get(ReferencedPackages).Take(1).Wait(),
             Is.EqualTo(new[] {FooReference}));
      }
    }

    private static Mock<NuGetPackageDownloader> MockDownloader(IEnumerable<PackageReference> packageReferences, TemporaryDirectory tmpDir) {
      var downloader = new Mock<NuGetPackageDownloader>(MockBehavior.Strict);
      var packagesCacheDir = Combine(tmpDir.Path, "build", "A", "cache");
      downloader.Setup(d => d.DownloadPackages(packageReferences, packagesCacheDir))
                .Returns(true);
      return downloader;
    }

    private static Mock<IAssemblyResolver> MockResolver(IEnumerable<PackageReference> packageReferences, IEnumerable<string> resolvedAssemblies, TemporaryDirectory tmpDir) {
      var resolver = new Mock<IAssemblyResolver>(MockBehavior.Strict);
      var baseDir = tmpDir.Path;
      var buildDir = Combine(baseDir, "build", "A");
      var packagesCacheDir = Combine(buildDir, "cache");
      resolver.Setup(r => r.FindAssembly(packageReferences,
                                         packagesCacheDir,
                                         buildDir))
              .Returns(resolvedAssemblies);
      return resolver;
    }

    private static IEnumerable<string> ReadResolvedAssembliesCache(IConf project)
      => ReadAllLines(Combine(("A"/Basic.BuildDir)[project], "resolved_assemblies")).Skip(1);
  }
}