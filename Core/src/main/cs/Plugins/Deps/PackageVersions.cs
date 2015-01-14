using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Plugins.Deps {
  public class PackageVersions {
    public readonly string Id;
    public readonly ImmutableList<Package> Versions;

    [JsonConstructor]
    public PackageVersions(string id, IEnumerable<Package> versions) {
      Id = id;
      Versions = versions.Select(package => package.WithId(Id)).ToImmutableList();
      Versions.Sort();
    }

    public PackageVersions(string id, IEnumerable<IPackage> downloadedPackages) : this(id, downloadedPackages.Select(package => new Package(package))) {}

    public Package GetBestSuitedVersion(SemanticVersion lowerBoundVersion) {
      if (Versions.Count > 0 && Versions[0].Version >= lowerBoundVersion) {
        return Versions[0];
      }
      return null;
    }

    public Package GetMostCurrentVersion() {
      return Versions.Count == 0 ? null : Versions[0];
    }
  }
}