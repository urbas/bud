using NuGet;
using System.Collections.Generic;

namespace Bud.Plugins.Dependencies {
  public class ResolvedNuGetDependency {
    public readonly ExternalDependency RequestedDependency;
    public readonly SemanticVersion ResolvedVersion;
    public readonly IEnumerable<string> AssemblyPaths;

    public ResolvedNuGetDependency(ExternalDependency requestedDependency, SemanticVersion resolvedVersion, IEnumerable<string> assemblyPaths) {
      this.AssemblyPaths = assemblyPaths;
      this.ResolvedVersion = resolvedVersion;
      this.RequestedDependency = requestedDependency;
    }
  }
}

