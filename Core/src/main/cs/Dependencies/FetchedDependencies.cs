using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class FetchedDependencies {
    public readonly ImmutableList<PackageVersions> Packages;
    [JsonIgnore] private readonly Dictionary<string, PackageVersions> packageId2PackageVersions;
    [JsonIgnore] internal IConfig Config;

    [JsonConstructor]
    public FetchedDependencies(IEnumerable<PackageVersions> packages) {
      Packages = packages == null ? ImmutableList<PackageVersions>.Empty : packages.ToImmutableList();
      packageId2PackageVersions = Packages.ToDictionary(packageVersions => packageVersions.Id, packageVersions => packageVersions.WithHostFetchedDependencies(this));
    }

    public FetchedDependencies(IEnumerable<IGrouping<string, IPackage>> fetchedPackages)
      : this(fetchedPackages.Select(packageGroup => new PackageVersions(packageGroup.Key, packageGroup))) {}

    public Package GetPackage(ExternalDependency dependency) {
      return GetPackage(dependency.Id, dependency.Version);
    }

    public Package GetPackage(string dependencyId, IVersionSpec versionRange) {
      return versionRange == null ? GetMostCurrentVersion(dependencyId) : GetBestSuitedVersion(dependencyId, versionRange);
    }

    private Package GetBestSuitedVersion(string dependencyId, IVersionSpec versionRange) {
      var bestSuitedVersion = GetAllVersionsForPackage(dependencyId).GetBestSuitedVersion(versionRange);
      if (bestSuitedVersion != null) {
        return bestSuitedVersion;
      }
      throw new Exception(string.Format("Could not find the version '{0}' of package '{1}'. Try running '{2}' to download packages.", dependencyId, versionRange, DependenciesKeys.FetchDependencies));
    }

    private Package GetMostCurrentVersion(string dependencyId) {
      var mostCurrentVersion = GetAllVersionsForPackage(dependencyId).GetMostCurrentVersion();
      if (mostCurrentVersion != null) {
        return mostCurrentVersion;
      }
      throw new Exception(PackageNotFoundMessage(dependencyId));
    }

    private PackageVersions GetAllVersionsForPackage(string id) {
      try {
        return packageId2PackageVersions[id];
      } catch (Exception e) {
        throw new ArgumentException(PackageNotFoundMessage(id), e);
      }
    }

    private static string PackageNotFoundMessage(string dependencyId) {
      return string.Format("Could not find any version of the package '{0}'. Try running '{1}' to download packages.", dependencyId, DependenciesKeys.FetchDependencies);
    }

    public FetchedDependencies WithConfig(IConfig config) {
      Config = config;
      return this;
    }
  }
}