using System;
using Bud.IO;
using NUnit.Framework;

namespace Bud.NuGet {
  public class NuGetPackageResolverTest {
    [Test]
    [Ignore("Takes too long")]
    public void ResolveAssemblies_installs_missing_packages() {
      using (var tmpDir = new TemporaryDirectory()) {
        var packagesConfigFile = PackageConfigTestUtils.CreatePackagesConfigFile(tmpDir, "Bud.packages.config");
        var assemblyResolver = new NuGetPackageResolver();
        var assemblies = assemblyResolver.Resolve(new[] {packagesConfigFile}, tmpDir.Path);
        Console.WriteLine($"{string.Join("\n", assemblies)}");
      }
    }
  }
}