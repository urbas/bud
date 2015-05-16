using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bud.Dependencies {
  public static class FetchedDependenciesUtil {
    public static IEnumerable<Package> MissingDependencies(FetchedDependencies fetchedDependencies) {
      return fetchedDependencies.Packages
                                .Where(versionOfPackage => versionOfPackage.AssemblyReferences.Any(assembly => !File.Exists(assembly.Path)));
    }

    public static string FetchedAssemblyAbsolutePath(string fetchedDependenciesDir, string packageId, string version, string assemblyRelativePath) {
      return Path.Combine(fetchedDependenciesDir, $"{packageId}.{version}", assemblyRelativePath);
    }
  }
}