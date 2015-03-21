using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class JsonFetchedDependencies {
    public readonly ImmutableList<JsonPackageVersions> Packages;

    [JsonConstructor]
    public JsonFetchedDependencies(IEnumerable<JsonPackageVersions> packages) {
      Packages = packages == null ? ImmutableList<JsonPackageVersions>.Empty : packages.ToImmutableList();
    }

    public JsonFetchedDependencies(IEnumerable<IGrouping<string, IPackage>> fetchedPackages)
      : this(fetchedPackages.Select(packageGroup => new JsonPackageVersions(packageGroup.Key, packageGroup))) {}
  }
}