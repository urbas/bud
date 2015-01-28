using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class Package {
    public readonly SemanticVersion Version;
    public readonly ImmutableList<AssemblyRereference> Assemblies;
    [JsonProperty(PropertyName = "Dependencies", NullValueHandling = NullValueHandling.Ignore)] private readonly ImmutableList<PackageInfo> dependencies;

    [JsonIgnore]
    public ImmutableList<PackageInfo> Dependencies {
      get { return dependencies ?? ImmutableList<PackageInfo>.Empty; }
    }

    [JsonConstructor]
    public Package(SemanticVersion version, IEnumerable<AssemblyRereference> assemblies, IEnumerable<PackageInfo> dependencies) {
      Version = version;  
      Assemblies = assemblies == null ? ImmutableList<AssemblyRereference>.Empty : assemblies.Select(assemblyReference => assemblyReference.WithHostPackage(this)).ToImmutableList();
      this.dependencies = dependencies == null || dependencies.IsEmpty() ? null : dependencies.ToImmutableList();
    }

    public Package(IPackage package) :
      this(package.Version,
           package.AssemblyReferences.Select(assemblyReference => new AssemblyRereference(assemblyReference)),
           package.DependencySets.SelectMany(dependencySet => dependencySet.Dependencies.Select(dependency => new PackageInfo(dependency, dependencySet.TargetFramework)))) {}

    [JsonIgnore]
    public string Id { get; internal set; }

    internal Package WithId(string packageId) {
      Id = packageId;
      return this;
    }
  }
}