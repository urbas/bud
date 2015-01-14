using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Plugins.Deps {
  public class Package {
    public readonly SemanticVersion Version;
    public readonly ImmutableList<AssemblyRereference> Assemblies;

    [JsonConstructor]
    public Package(SemanticVersion version, IEnumerable<AssemblyRereference> assemblies) {
      Assemblies = assemblies.Select(assemblyReference => assemblyReference.WithHostPackage(this)).ToImmutableList();
      Version = version;
    }

    public Package(IPackage package) : this(package.Version, package.AssemblyReferences.Select(assemblyReference => new AssemblyRereference(assemblyReference))) {
    }

    [JsonIgnore]
    public string Id { get; internal set; }

    internal Package WithId(string packageId) {
      Id = packageId;
      return this;
    }
  }
}