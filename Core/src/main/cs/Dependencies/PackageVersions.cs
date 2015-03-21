using System.Collections.Immutable;
using System.Linq;
using NuGet;

namespace Bud.Dependencies {
  public class PackageVersions {
    public readonly ImmutableList<Package> Versions;
    public readonly string Id;

    public PackageVersions(IConfig config, JsonPackageVersions package) {
      Versions = package.Versions.Select(packageVersion => new Package(package.Id, packageVersion, config)).ToImmutableList();
      Id = package.Id;
    }

    public Package GetBestSuitedVersion(IVersionSpec versionRange) {
      return Versions.Count > 0 ? Versions.Find(package => versionRange.Satisfies(package.Version)) : null;
    }

    public Package GetMostCurrentVersion() {
      return Versions.Count == 0 ? null : Versions[0];
    }
  }
}