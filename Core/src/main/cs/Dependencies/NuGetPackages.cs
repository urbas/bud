using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class NuGetPackages {
    public readonly ImmutableList<PackageVersions> Packages;
    [JsonIgnore] private readonly Dictionary<string, PackageVersions> packageId2PackageVersions;

    [JsonConstructor]
    public NuGetPackages(IEnumerable<PackageVersions> packages) {
      Packages = packages == null ? ImmutableList<PackageVersions>.Empty : packages.ToImmutableList();
      packageId2PackageVersions = Packages.ToDictionary(packageVersions => packageVersions.Id, packageVersions => packageVersions);
    }

    public NuGetPackages(IEnumerable<IGrouping<string, IPackage>> fetchedPackages)
      : this(fetchedPackages.Select(packageGroup => new PackageVersions(packageGroup.Key, packageGroup))) {}

    public Package GetResolvedNuGetDependency(ExternalDependency dependency) {
      return GetResolvedNuGetDependency(dependency.Id, dependency.Version);
    }

    public Package GetResolvedNuGetDependency(string dependencyId, SemanticVersion lowerBoundVersion) {
      if (lowerBoundVersion == null) {
        var mostCurrentVersion = GetAllVersionsForPackage(dependencyId).GetMostCurrentVersion();
        if (mostCurrentVersion != null) {
          return mostCurrentVersion;
        }
        throw new Exception(PackageNotFoundMessage(dependencyId));
      }

      var bestSuitedVersion = GetAllVersionsForPackage(dependencyId).GetBestSuitedVersion(lowerBoundVersion);
      if (bestSuitedVersion != null) {
        return bestSuitedVersion;
      }
      throw new Exception(string.Format("Could not find the version '{0}' of package '{1}'. Try running '{2}' to download packages.", dependencyId, lowerBoundVersion, DependenciesKeys.Fetch));
    }

    private PackageVersions GetAllVersionsForPackage(string id) {
      try {
        return packageId2PackageVersions[id];
      } catch (Exception e) {
        throw new ArgumentException(PackageNotFoundMessage(id), e);
      }
    }

    private static string PackageNotFoundMessage(string dependencyId) {
      return string.Format("Could not find any version of the package '{0}'. Try running '{1}' to download packages.", dependencyId, DependenciesKeys.Fetch);
    }
  }
}