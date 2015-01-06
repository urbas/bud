using NuGet;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System;

namespace Bud.Plugins.NuGet {
  public class ResolvedNuGetDependency {
    public readonly NuGetDependency RequestedDependency;
    public readonly SemanticVersion ResolvedVersion;
    public readonly IEnumerable<string> AssemblyPaths;

    public ResolvedNuGetDependency(NuGetDependency requestedDependency, SemanticVersion resolvedVersion, IEnumerable<string> assemblyPaths) {
      this.AssemblyPaths = assemblyPaths;
      this.ResolvedVersion = resolvedVersion;
      this.RequestedDependency = requestedDependency;
    }
  }
}

