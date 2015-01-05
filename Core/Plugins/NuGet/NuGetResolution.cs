using NuGet;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.Immutable;

namespace Bud.Plugins.NuGet {

	public class NuGetResolution	{
    public readonly IDictionary<string, Dictionary<SemanticVersion, IEnumerable<string>>> fetchedPackages;

    [JsonConstructor]
    public NuGetResolution(IDictionary<string, Dictionary<SemanticVersion, IEnumerable<string>>> fetchedPackages) {
      this.fetchedPackages = fetchedPackages;
    }

    public NuGetResolution(IEnumerable<IGrouping<string, IPackage>> fetchedPackages) : this(ToJsonSerializable(fetchedPackages)) {
    }

    private static IDictionary<string, Dictionary<SemanticVersion, IEnumerable<string>>> ToJsonSerializable(IEnumerable<IGrouping<string, IPackage>> fetchedPackages) {
      return fetchedPackages
        .ToDictionary(
          packageGroup => packageGroup.Key,
          packageGroup => packageGroup.ToDictionary(
            package => package.Version,
            package => package.AssemblyReferences.Select(assemblyReference => assemblyReference.Path)
          )
        );
    }

    public IEnumerable<string> GetPackageAssemblyPaths(NuGetDependency dependency) {
      return fetchedPackages[dependency.PackageName].First(versionToPaths => versionToPaths.Key >= dependency.PackageVersion).Value;
    }
	}
}

