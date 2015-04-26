using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class JsonPackage {
    public readonly SemanticVersion Version;
    public readonly ImmutableList<JsonAssembly> Assemblies;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public readonly ImmutableList<PackageDependencyInfo> Dependencies;

    [JsonConstructor]
    public JsonPackage(SemanticVersion version, IEnumerable<JsonAssembly> assemblies, IEnumerable<PackageDependencyInfo> dependencies)
    {
      Version = version;
      Assemblies = assemblies == null ? ImmutableList<JsonAssembly>.Empty : assemblies.ToImmutableList();
      Dependencies = dependencies == null || dependencies.IsEmpty() ? null : dependencies.ToImmutableList();
    }

    public JsonPackage(IPackage package) :
      this(package.Version,
           package.AssemblyReferences.Select(assemblyReference => new JsonAssembly(assemblyReference)),
           package.DependencySets.SelectMany(dependencySet => dependencySet.Dependencies.Select(dependency => new PackageDependencyInfo(dependency, dependencySet.TargetFramework, dependencySet.SupportedFrameworks)))) { }
  }
}