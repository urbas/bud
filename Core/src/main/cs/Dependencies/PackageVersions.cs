using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class PackageVersions {
    public readonly string Id;
    public readonly ImmutableList<Package> Versions;

    [JsonConstructor]
    public PackageVersions(string id, IEnumerable<Package> versions) {
      Id = id;
      Versions = versions == null ? ImmutableList<Package>.Empty : versions.Select(package => package.WithId(Id))
                                                                           .OrderByDescending(package => package.Version)
                                                                           .ToImmutableList();
    }

    public PackageVersions(string id, IEnumerable<IPackage> downloadedPackages) : this(id, downloadedPackages.Select(package => new Package(package))) {}

    public Package GetBestSuitedVersion(IVersionSpec versionRange) {
      return Versions.Count > 0 ? Versions.Find(package => versionRange.Satisfies(package.Version)) : null;
    }

    public Package GetMostCurrentVersion() {
      return Versions.Count == 0 ? null : Versions[0];
    }
  }
}