using System;
using System.Collections.Immutable;
using System.Linq;
using NuGet;

namespace Bud.Dependencies {
  public class FetchedDependencies {
    private readonly ImmutableDictionary<string, ImmutableList<Package>> PackageId2PackageVersions;

    public FetchedDependencies(IConfig config, JsonFetchedDependencies jsonFetchedDependencies) {
      Packages = jsonFetchedDependencies.Packages
                                        .SelectMany(packageVersions => packageVersions.Versions.Select(package => new Package(config.GetFetchedDependenciesDir(), packageVersions.Id, package)))
                                        .ToImmutableList();
      PackageId2PackageVersions = Packages.GroupBy(package => package.Id)
                                          .ToImmutableDictionary(package => package.Key, package => package.ToImmutableList());
    }

    public ImmutableList<Package> Packages { get; }

    public Package GetPackage(ExternalDependency dependency) {
      return GetPackage(dependency.Id, dependency.Version);
    }

    public Package GetPackage(string dependencyId, IVersionSpec versionRange) {
      return versionRange == null ? GetMostCurrentVersion(dependencyId) : GetBestSuitedVersion(dependencyId, versionRange);
    }

    private Package GetBestSuitedVersion(string dependencyId, IVersionSpec versionRange) {
      var allVersionsOfPackage = GetAllVersionsOfPackage(dependencyId);
      var bestSuitedVersion = GetBestSuitedVersion(versionRange, allVersionsOfPackage);
      if (bestSuitedVersion != null) {
        return bestSuitedVersion;
      }
      throw new Exception(string.Format("Could not find the version '{0}' of package '{1}'. Try running '{2}' to download packages.", dependencyId, versionRange, DependenciesKeys.Fetch));
    }

    private Package GetMostCurrentVersion(string dependencyId) {
      var allVersionsOfPackage = GetAllVersionsOfPackage(dependencyId);
      var mostCurrentVersion = GetMostCurrentVersion(allVersionsOfPackage);
      if (mostCurrentVersion != null) {
        return mostCurrentVersion;
      }
      throw new Exception(PackageNotFoundMessage(dependencyId));
    }

    private ImmutableList<Package> GetAllVersionsOfPackage(string id) {
      try {
        return PackageId2PackageVersions[id];
      } catch (Exception e) {
        throw new ArgumentException(PackageNotFoundMessage(id), e);
      }
    }

    private static Package GetBestSuitedVersion(IVersionSpec versionRange, ImmutableList<Package> allVersionsOfPackage) {
      return allVersionsOfPackage.Count > 0 ? allVersionsOfPackage.Find(package => versionRange.Satisfies(package.Version)) : null;
    }

    private static Package GetMostCurrentVersion(ImmutableList<Package> allVersionsOfPackage) {
      return allVersionsOfPackage.Count == 0 ? null : allVersionsOfPackage[0];
    }

    private static string PackageNotFoundMessage(string dependencyId) {
      return string.Format("Could not find any version of the package '{0}'. Try running '{1}' to download packages.", dependencyId, DependenciesKeys.Fetch);
    }
  }
}