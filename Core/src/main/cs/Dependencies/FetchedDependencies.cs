using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NuGet;

namespace Bud.Dependencies {
  public class FetchedDependencies {
    private readonly Dictionary<string, PackageVersions> PackageId2PackageVersions;
    public readonly IConfig Config;
    private readonly JsonFetchedDependencies JsonFetchedDependencies;

    public FetchedDependencies(IConfig config, JsonFetchedDependencies jsonFetchedDependencies) {
      PackageId2PackageVersions = jsonFetchedDependencies.Packages.ToDictionary(packageVersions => packageVersions.Id, packageVersions => new PackageVersions(config, packageVersions));
      Config = config;
      JsonFetchedDependencies = jsonFetchedDependencies;
      Packages = JsonFetchedDependencies.Packages.Select(package => new PackageVersions(config, package)).ToImmutableList();
    }

    public ImmutableList<PackageVersions> Packages { get; }

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
      throw new Exception(string.Format("Could not find the version '{0}' of package '{1}'. Try running '{2}' to download packages.", dependencyId, versionRange, DependenciesKeys.Fetch));
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
        return PackageId2PackageVersions[id];
      } catch (Exception e) {
        throw new ArgumentException(PackageNotFoundMessage(id), e);
      }
    }

    private static string PackageNotFoundMessage(string dependencyId) {
      return string.Format("Could not find any version of the package '{0}'. Try running '{1}' to download packages.", dependencyId, DependenciesKeys.Fetch);
    }
  }
}