using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Bud.V1;
using Moq;
using NUnit.Framework;
using static System.IO.File;
using static System.IO.Path;
using static Bud.NuGet.NuGetPackageReferencesReader;
using static Bud.NuGet.PackageConfigTestUtils;
using static Bud.V1.Api;
using static NUnit.Framework.Assert;

namespace Bud.NuGet {
  public class PackageReferencesProjectsTest {
    [Test]
    public void Packages_config_file_is_at_the_root_by_default()
      => That(PackagesConfigFile[TestProject("fooDir")],
              Is.EqualTo(Combine(Directory.GetCurrentDirectory(), "fooDir", "packages.config")));

    [Test]
    public void Assemblies_is_initially_empty()
      => That(ResolvedAssemblies[TestProject("fooDir")].Take(1).ToEnumerable(),
              Has.Exactly(1).Empty);

    [Test]
    [Category("IntegrationTest")]
    public void PackageDownloader_puts_assemblies_into_the_target_folder() {
      using (var tmpDir = new TemporaryDirectory()) {
        CreatePackagesConfigFile(tmpDir);
        var resolvedAssemblies = ImmutableList.Create("Foo.dll", "Bar.dll");
        var resolver = MockResolver(new[] {FooReference}, resolvedAssemblies, tmpDir);
        var project = TestProject(tmpDir.Path)
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
        var project = TestProject(tmpDir.Path)
          .Clear(ReferencedPackages)
          .Set(PackageDownloader, downloader.Object);

        ResolvedAssemblies[project].Take(1).Wait();
      }
    }

    [Test]
    public void PackageDownloader_and_AssemblyResolver_are_invoked_with_a_list_of_package_references() {
      using (var tmpDir = new TemporaryDirectory()) {
        CreatePackagesConfigFile(tmpDir);
        var expectedAssemblies = ImmutableList.Create("Foo.dll");
        var downloader = MockDownloader(new[] {FooReference}, tmpDir);
        var resolver = MockResolver(new[] {FooReference}, expectedAssemblies, tmpDir);
        var project = TestProject(tmpDir.Path)
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
      using (var tmpDir = new TemporaryDirectory()) {
        CreatePackagesConfigFile(tmpDir);
        var resolver = new Mock<IAssemblyResolver>(MockBehavior.Strict);
        var project = TestProject(tmpDir.Path)
          .Set(AssemblyResolver, resolver.Object)
          .ToCompiled();
        tmpDir.CreateFile(
          "4D-31-2B-41-83-A6-87-D8-FC-8C-92-C7-F3-CE-60-E9\nMoo.dll\nZoo.dll",
          ("A"/BuildDir)[project], "resolved_assemblies");

        ("A"/ResolvedAssemblies)[project].Take(1).Wait();

        That(ReadResolvedAssembliesCache(project),
             Is.EquivalentTo(new[] {"Moo.dll", "Zoo.dll"}));
      }
    }

    [Test]
    public void ReferencedPackages_lists_package_references_read_from() {
      using (var tmpDir = new TemporaryDirectory()) {
        CreatePackagesConfigFile(tmpDir);
        var project = TestProject(tmpDir.Path);
        That(project.Get(ReferencedPackages).Take(1).Wait(),
             Is.EqualTo(new[] {FooReference}));
      }
    }

    private static Conf TestProject(string baseDir)
      => PackageReferencesProject(baseDir, "A");

    private static Mock<IAssemblyResolver> MockPackageResolver(string packageConfigFile,
                                                               IEnumerable<string> assemblies) {
      var resolver = new Mock<IAssemblyResolver>(MockBehavior.Strict);
      var packageReferences = LoadReferences(packageConfigFile);
      resolver.Setup(self => self.FindAssembly(packageReferences,
                                               It.IsAny<string>(),
                                               It.IsAny<string>()))
              .Returns(assemblies.ToImmutableHashSet());
      return resolver;
    }

    private static Mock<NuGetPackageDownloader> MockDownloader(IEnumerable<PackageReference> packageReferences, TemporaryDirectory tmpDir) {
      var downloader = new Mock<NuGetPackageDownloader>(MockBehavior.Strict);
      var packagesCacheDir = Combine(tmpDir.Path, "cache");
      downloader.Setup(d => d.DownloadPackages(packageReferences, packagesCacheDir))
                .Returns(true);
      return downloader;
    }

    private static Mock<IAssemblyResolver> MockResolver(IEnumerable<PackageReference> packageReferences, IEnumerable<string> resolvedAssemblies, TemporaryDirectory tmpDir) {
      var resolver = new Mock<IAssemblyResolver>(MockBehavior.Strict);
      var projectDir = tmpDir.Path;
      var packagesCacheDir = Combine(projectDir, "cache");
      resolver.Setup(r => r.FindAssembly(packageReferences,
                                         packagesCacheDir,
                                         projectDir))
              .Returns(resolvedAssemblies);
      return resolver;
    }

    private static IEnumerable<string> ReadResolvedAssembliesCache(IConf project)
      => ReadAllLines(Combine(("A"/BuildDir)[project], "resolved_assemblies")).Skip(1);
  }
}