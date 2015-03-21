using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bud.Dependencies {
  public static class FetchedDependenciesUtil {
    public static IEnumerable<Package> MissingDependencies(FetchedDependencies fetchedDependencies) {
      return fetchedDependencies.Packages
                                .SelectMany(package => package.Versions)
                                .Where(AnyAssemblyMissing);
    }

    private static bool AnyAssemblyMissing(Package versionOfPackage) {
      return versionOfPackage.AssemblyReferences.Any(assembly => !File.Exists(assembly.Path));
    }
  }
}