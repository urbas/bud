using NuGet;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System;
using Bud.Plugins.Dependencies;

namespace Bud.Plugins.Dependencies {

	public class ResolvedExternalDependencies	{
    public readonly IDictionary<string, IDictionary<SemanticVersion, IEnumerable<string>>> fetchedPackages;

    [JsonConstructor]
    public ResolvedExternalDependencies(IDictionary<string, IDictionary<SemanticVersion, IEnumerable<string>>> fetchedPackages) {
      this.fetchedPackages = fetchedPackages;
    }

    public ResolvedExternalDependencies(IEnumerable<IGrouping<string, IPackage>> fetchedPackages) : this(ToJsonSerializable(fetchedPackages)) {
    }

    private static IDictionary<string, IDictionary<SemanticVersion, IEnumerable<string>>> ToJsonSerializable(IEnumerable<IGrouping<string, IPackage>> fetchedPackages) {
      return fetchedPackages
        .ToDictionary(
          packageGroup => packageGroup.Key,
          packageGroup => (IDictionary<SemanticVersion, IEnumerable<string>>)packageGroup.ToDictionary(
            package => package.Version,
            package => package.AssemblyReferences.Select(assemblyReference => assemblyReference.Path)
          )
        );
    }

    public ResolvedExternalDependency GetResolvedNuGetDependency(ExternalDependency dependency) {
      if (dependency.Version == null) {
        var packagePaths = fetchedPackages[dependency.Id].GetEnumerator();
        if (packagePaths.MoveNext()) {
          var bestPackagePaths = packagePaths.Current;
          while (packagePaths.MoveNext()) {
            var currentPackagePaths = packagePaths.Current;
            if (currentPackagePaths.Key > bestPackagePaths.Key) {
              bestPackagePaths = currentPackagePaths;
            }
          }
          return new ResolvedExternalDependency(dependency, bestPackagePaths.Key, bestPackagePaths.Value);
        }
        throw new Exception(string.Format("Could not find any version of the package '{0}'. Try running '{1}' to download packages.", dependency.Id, DependenciesKeys.Fetch));
      }

      var suitablePackages = fetchedPackages[dependency.Id]
        .Where(versionToPaths => versionToPaths.Key >= dependency.Version)
        .Select(versionToPaths => new ResolvedExternalDependency(dependency, versionToPaths.Key, versionToPaths.Value));

      foreach (var suitablePackage in suitablePackages) {
        return suitablePackage;
      }

      throw new Exception(string.Format("Could not find the version '{0}' of package '{1}'. Try running '{2}' to download packages.", dependency.Id, dependency.Version, DependenciesKeys.Fetch));
    }
	}
}

