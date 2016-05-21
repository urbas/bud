using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.NuGet;
using Bud.TempDir;
using Moq;
using NUnit.Framework;
using static Bud.V1.NuGetReferences;

namespace Bud.V1 {
  public class NuGetReferencesTest {
    [Test]
    public void Packages_config_file_is_in_ProjectDir_by_default()
      => Assert.That(PackageReferencesProject("A", baseDir: "/foo").Get(PackagesConfigFile),
                     Is.EqualTo(Path.Combine("/foo", "A", "packages.config")));

    [Test]
    public void Assemblies_is_initially_empty() {
      using (var dir = new TemporaryDirectory()) {
        Assert.That(PackageReferencesProject("A", baseDir: dir.Path)
                      .Get(ResolvedAssemblies)
                      .Take(1).ToEnumerable(),
                    Has.Exactly(1).Empty);
      }
    }

    [Test]
    [Category("IntegrationTest")]
    public void PackageDownloader_puts_assemblies_into_the_target_folder() {
      using (var dir = new TemporaryDirectory()) {
        PackageConfigTestUtils.CreatePackagesConfigFile(dir);
        var resolvedAssemblies = ImmutableList.Create("Foo.dll", "Bar.dll");
        var resolver = MockResolver(new[] {PackageConfigTestUtils.FooReference}, resolvedAssemblies, dir);
        var project = PackageReferencesProject("A", ".", dir.Path)
          .Set(AssemblyResolver, resolver.Object)
          .ToCompiled();

        ("A"/ResolvedAssemblies)[project].Take(1).Wait();

        Assert.That(ReadResolvedAssembliesCache(project),
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
        PackageConfigTestUtils.CreatePackagesConfigFile(dir);
        var expectedAssemblies = ImmutableList.Create("Foo.dll");
        var downloader = MockDownloader(new[] {PackageConfigTestUtils.FooReference}, dir);
        var resolver = MockResolver(new[] {PackageConfigTestUtils.FooReference}, expectedAssemblies, dir);
        var project = PackageReferencesProject("A", ".", dir.Path)
          .Set(PackageDownloader, downloader.Object)
          .Set(AssemblyResolver, resolver.Object);

        var actualAssemblies = ResolvedAssemblies[project].Take(1).ToEnumerable();

        Assert.That(actualAssemblies, Has.Exactly(1).EqualTo(expectedAssemblies));
        resolver.VerifyAll();
        downloader.VerifyAll();
      }
    }

    [Test]
    public void Assemblies_are_loaded_from_cache() {
      using (var dir = new TemporaryDirectory()) {
        PackageConfigTestUtils.CreatePackagesConfigFile(dir);
        var resolver = new Mock<IAssemblyResolver>(MockBehavior.Strict);
        var project = PackageReferencesProject("A", ".", dir.Path)
          .Set(AssemblyResolver, resolver.Object)
          .ToCompiled();
        dir.CreateFile(
          "4D-31-2B-41-83-A6-87-D8-FC-8C-92-C7-F3-CE-60-E9\nMoo.dll\nZoo.dll",
          project.Get("A"/Basic.BuildDir), "resolved_assemblies");

        ("A"/ResolvedAssemblies)[project].Take(1).Wait();

        Assert.That(ReadResolvedAssembliesCache(project),
                    Is.EquivalentTo(new[] {"Moo.dll", "Zoo.dll"}));
      }
    }

    [Test]
    public void ReferencedPackages_lists_package_references_read_from() {
      using (var dir = new TemporaryDirectory()) {
        PackageConfigTestUtils.CreatePackagesConfigFile(dir);
        var project = PackageReferencesProject("A", ".", dir.Path);
        Assert.That(project.Get(ReferencedPackages).Take(1).Wait(),
                    Is.EqualTo(new[] {PackageConfigTestUtils.FooReference}));
      }
    }

    private static Mock<NuGetPackageDownloader> MockDownloader(IEnumerable<PackageReference> packageReferences, TemporaryDirectory tmpDir) {
      var downloader = new Mock<NuGetPackageDownloader>(MockBehavior.Strict);
      var packagesCacheDir = Path.Combine(tmpDir.Path, "build", "A", "cache");
      downloader.Setup(d => d.DownloadPackages(packageReferences, packagesCacheDir))
                .Returns(true);
      return downloader;
    }

    private static Mock<IAssemblyResolver> MockResolver(IEnumerable<PackageReference> packageReferences, IEnumerable<string> resolvedAssemblies, TemporaryDirectory tmpDir) {
      var resolver = new Mock<IAssemblyResolver>(MockBehavior.Strict);
      var baseDir = tmpDir.Path;
      var buildDir = Path.Combine(baseDir, "build", "A");
      var packagesCacheDir = Path.Combine(buildDir, "cache");
      resolver.Setup(r => r.FindAssembly(packageReferences,
                                         packagesCacheDir,
                                         buildDir))
              .Returns(resolvedAssemblies);
      return resolver;
    }

    private static IEnumerable<string> ReadResolvedAssembliesCache(IConf project)
      => File.ReadAllLines(Path.Combine(("A"/Basic.BuildDir)[project], "resolved_assemblies")).Skip(1);
  }
}