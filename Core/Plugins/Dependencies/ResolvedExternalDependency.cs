using NuGet;
using System.Collections.Generic;

namespace Bud.Plugins.Dependencies {
  public class ResolvedExternalDependency {
    public readonly ExternalDependency RequestedDependency;
    public readonly SemanticVersion ResolvedVersion;
    public readonly IEnumerable<string> AssemblyPaths;

    public ResolvedExternalDependency(ExternalDependency requestedDependency, SemanticVersion resolvedVersion, IEnumerable<string> assemblyPaths) {
      this.AssemblyPaths = assemblyPaths;
      this.ResolvedVersion = resolvedVersion;
      this.RequestedDependency = requestedDependency;
    }
  }
}