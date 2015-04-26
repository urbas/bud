using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class JsonPackageVersions {
    public readonly string Id;
    public readonly ImmutableList<JsonPackage> Versions;

    [JsonConstructor]
    public JsonPackageVersions(string id, IEnumerable<JsonPackage> versions) {
      Id = id;
      Versions = versions == null ? ImmutableList<JsonPackage>.Empty : versions.OrderByDescending(package => package.Version).ToImmutableList();
    }

    public JsonPackageVersions(string id, IEnumerable<IPackage> downloadedPackages) : this(id, downloadedPackages.Select(package => new JsonPackage(package))) {}
  }
}