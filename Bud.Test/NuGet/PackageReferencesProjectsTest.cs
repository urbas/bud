using System.Collections.Generic;
using System.Collections.Immutable;
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
    public void Packages_config_file_is_at_the_root_by_default()
      => That(PackagesConfigFile[TestProject()],
              Is.EqualTo(Combine("a", "packages.config")));

    [Test]
    public void Assemblies_is_initially_empty()
      => That(ResolvedAssemblies[TestProject()].Take(1).ToEnumerable(),
              Has.Exactly(1).Empty);

    [Test]
    public void Assemblies_are_resolved_from_the_packages_config_file() {
      using (var tmpDir = new TemporaryDirectory()) {
        var packageConfigFile = CreatePackagesConfigFile(tmpDir);
        var expectedAssemblies = ImmutableList.Create("Foo.dll");
        var resolver = MockPackageResolver(packageConfigFile, expectedAssemblies);
        var project = TestProject(tmpDir.Path)
          .SetValue(AssemblyResolver, resolver.Object);

        var actualAssemblies = ResolvedAssemblies[project].Take(1).ToEnumerable();

        That(actualAssemblies, Has.Exactly(1).EqualTo(expectedAssemblies));
        resolver.VerifyAll();
      }
    }

    [Test]
    public void Assemblies_are_stored_in_the_target_folder() {
      using (var tmpDir = new TemporaryDirectory()) {
        var packageConfigFile = CreatePackagesConfigFile(tmpDir);
        var resolvedAssemblies = ImmutableList.Create("Foo.dll", "Bar.dll");
        var resolver = MockPackageResolver(packageConfigFile, resolvedAssemblies);
        var project = TestProject(tmpDir.Path)
          .SetValue(AssemblyResolver, resolver.Object)
          .ToCompiled();

        ("A"/ResolvedAssemblies)[project].Take(1).Wait();

        That(ReadResolvedAssembliesCache(project),
             Is.EquivalentTo(resolvedAssemblies));
      }
    }

    [Test]
    public void Assemblies_are_loaded_from_cache() {
      using (var tmpDir = new TemporaryDirectory()) {
        CreatePackagesConfigFile(tmpDir);
        var resolver = new Mock<IPackageResolver>(MockBehavior.Strict);
        var project = TestProject(tmpDir.Path)
          .SetValue(AssemblyResolver, resolver.Object)
          .ToCompiled();
        tmpDir.CreateFile("Moo.dll\nZoo.dll", ("A"/BudDir)[project], "resolved_assemblies");

        ("A"/ResolvedAssemblies)[project].Take(1).Wait();

        That(ReadResolvedAssembliesCache(project),
             Is.EquivalentTo(new[] {"Moo.dll", "Zoo.dll"}));
      }
    }

    private static Conf TestProject(string baseDir = "a")
      => PackageReferencesProject(baseDir, "A");

    private static Mock<IPackageResolver> MockPackageResolver(string packageConfigFile,
                                                              IEnumerable<string> assemblies) {
      var resolver = new Mock<IPackageResolver>(MockBehavior.Strict);
      resolver.Setup(self => self.Resolve(new[] {packageConfigFile},
                                          It.IsAny<string>()))
              .Returns(assemblies.ToImmutableHashSet());
      return resolver;
    }

    private static string[] ReadResolvedAssembliesCache(IConf project)
      => ReadAllLines(Combine(("A"/BudDir)[project], "resolved_assemblies"));
  }
}