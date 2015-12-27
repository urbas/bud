using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Bud.IO;
using Bud.NuGet;
using Moq;
using NUnit.Framework;
using static System.IO.File;
using static System.IO.Path;
using static Bud.V1.Api;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  public class PackageReferencesProjectTest {
    [Test]
    public void Packages_config_file_is_at_the_root_by_default()
      => That(PackagesConfigFile[TestProject()],
              Is.EqualTo(Combine("a", "packages.config")));

    [Test]
    public void Assemblies_is_initially_empty()
      => That(Assemblies[TestProject()].Take(1).ToEnumerable(),
              Has.Exactly(1).Empty);

    [Test]
    public void Assemblies_are_resolved_from_the_packages_config_file() {
      using (var tmpDir = new TemporaryDirectory()) {
        var packageConfigFile = PackageConfigTestUtils.CreatePackagesConfigFile(tmpDir);
        var expectedAssemblies = ImmutableList.Create("Foo.dll");
        var assemblyResolver = MockAssemblyResolver(packageConfigFile, expectedAssemblies);
        var project = TestProject(tmpDir.Path)
          .SetValue(AssemblyResolver, assemblyResolver.Object);

        var actualAssemblies = Assemblies[project].Take(1).ToEnumerable();

        That(actualAssemblies, Has.Exactly(1).EqualTo(expectedAssemblies));
        assemblyResolver.VerifyAll();
      }
    }

    [Test]
    public void Assemblies_are_stored_in_the_target_folder() {
      using (var tmpDir = new TemporaryDirectory()) {
        var packageConfigFile = PackageConfigTestUtils.CreatePackagesConfigFile(tmpDir);
        var resolvedAssemblies = ImmutableList.Create("Foo.dll", "Bar.dll");
        var assemblyResolver = MockAssemblyResolver(packageConfigFile, resolvedAssemblies);
        var project = TestProject(tmpDir.Path)
          .SetValue(AssemblyResolver, assemblyResolver.Object)
          .ToCompiled();

        ("A"/Assemblies)[project].Take(1).Wait();

        That(ReadResolvedAssembliesCache(project),
             Is.EquivalentTo(resolvedAssemblies));
      }
    }

    [Test]
    public void Assemblies_are_loaded_from_cache() {
      using (var tmpDir = new TemporaryDirectory()) {
        PackageConfigTestUtils.CreatePackagesConfigFile(tmpDir);
        var assemblyResolver = new Mock<IAssemblyResolver>(MockBehavior.Strict);
        var project = TestProject(tmpDir.Path)
          .SetValue(AssemblyResolver, assemblyResolver.Object)
          .ToCompiled();
        tmpDir.CreateFile("Moo.dll\nZoo.dll", ("A"/TargetDir)[project], "resolved_assemblies");

        ("A"/Assemblies)[project].Take(1).Wait();

        That(ReadResolvedAssembliesCache(project),
             Is.EquivalentTo(new[] {"Moo.dll", "Zoo.dll"}));
      }
    }

    private static Conf TestProject(string baseDir = "a")
      => PackageReferencesProject(baseDir, "A");

    private static Mock<IAssemblyResolver> MockAssemblyResolver(string packageConfigFile,
                                                                IEnumerable<string> assemblies) {
      var assemblyResolver = new Mock<IAssemblyResolver>(MockBehavior.Strict);
      assemblyResolver.Setup(self => self.ResolveAssemblies(new[] {packageConfigFile},
                                                            It.IsAny<string>()))
                      .Returns(new ResolvedAssemblies(ImmutableHashSet<string>.Empty, assemblies.ToImmutableHashSet()));
      return assemblyResolver;
    }

    private static string[] ReadResolvedAssembliesCache(IConf project)
      => ReadAllLines(Combine(("A"/TargetDir)[project], "resolved_assemblies"));
  }
}