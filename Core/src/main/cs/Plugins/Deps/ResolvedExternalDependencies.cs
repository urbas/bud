using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Plugins.Deps {
  public class ResolvedExternalDependencies {
    public readonly ImmutableList<PackageVersions> Packages;
    [JsonIgnore] private readonly Dictionary<string, PackageVersions> packageId2PackageVersions;

    [JsonConstructor]
    public ResolvedExternalDependencies(IEnumerable<PackageVersions> packages) {
      Packages = packages.ToImmutableList();
      packageId2PackageVersions = Packages.ToDictionary(packageVersions => packageVersions.Id, packageVersions => packageVersions);
    }

    public ResolvedExternalDependencies(IEnumerable<IGrouping<string, IPackage>> fetchedPackages)
      : this(fetchedPackages.Select(packageGroup => new PackageVersions(packageGroup.Key, packageGroup))) {}

    public ResolvedExternalDependency GetResolvedNuGetDependency(ExternalDependency dependency) {
      if (dependency.Version == null) {
        var mostCurrentVersion = GetAllVersionsForPackage(dependency.Id).GetMostCurrentVersion();
        if (mostCurrentVersion != null) {
          return new ResolvedExternalDependency(dependency, mostCurrentVersion);
        }
        throw new Exception(string.Format("Could not find any version of the package '{0}'. Try running '{1}' to download packages.", dependency.Id, DependenciesKeys.Fetch));
      }

      var bestSuitedVersion = GetAllVersionsForPackage(dependency.Id).GetBestSuitedVersion(dependency.Version);
      if (bestSuitedVersion != null) {
        return new ResolvedExternalDependency(dependency, bestSuitedVersion);
      }
      throw new Exception(string.Format("Could not find the version '{0}' of package '{1}'. Try running '{2}' to download packages.", dependency.Id, dependency.Version, DependenciesKeys.Fetch));
    }

    private PackageVersions GetAllVersionsForPackage(string id) {
      return packageId2PackageVersions[id];
    }
  }
}