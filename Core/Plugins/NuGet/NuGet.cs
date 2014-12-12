using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;
using System.Threading.Tasks;
using Bud.Commander;
using Bud.Plugins.Dependencies;

namespace Bud.Plugins.NuGet {
  public static class NuGet {
    public static Settings NeedsNuGet(this Settings dependent, string packageName, string packageVersion) {
      return dependent.Needs(new NuGetDependency(packageName, packageVersion));
    }
  }
}

