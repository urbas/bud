using System.Collections.Generic;
using NuGet;

namespace Bud.Plugins.Deps {
  public class ResolvedExternalDependency {
    public readonly ExternalDependency RequestedDependency;
    public readonly Package Package;

    public ResolvedExternalDependency(ExternalDependency requestedDependency, Package package) {
      RequestedDependency = requestedDependency;
      Package = package;
    }
  }
}