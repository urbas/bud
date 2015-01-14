using System.Collections.Generic;
using NuGet;

namespace Bud.Plugins.Deps {
  public class ResolvedExternalDependency {
    public readonly ExternalDependency RequestedDependency;
    public readonly SemanticVersion ResolvedVersion;
    public readonly IEnumerable<string> AssemblyPaths;

    public ResolvedExternalDependency(ExternalDependency requestedDependency, SemanticVersion resolvedVersion, IEnumerable<string> assemblyPaths) {
      AssemblyPaths = assemblyPaths;
      ResolvedVersion = resolvedVersion;
      RequestedDependency = requestedDependency;
    }

    public ResolvedExternalDependency(ExternalDependency requestedDependency, DownloadedPackage downloadedPackage) {
      AssemblyPaths = downloadedPackage.AssemblyPaths;
      ResolvedVersion = downloadedPackage.Version;
      RequestedDependency = requestedDependency;
    }
  }
}