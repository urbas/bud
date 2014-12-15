using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;
using Bud.Commander;
using System.Threading.Tasks;
using Bud.Plugins.Dependencies;

namespace Bud.Plugins.NuGet
{
  public class NuGetResolvedDependency : IResolvedDependency<NuGetPackage>
	{
    public NuGetResolvedDependency(NuGetDependency nuGetDependency) {
      Dependency = new NuGetPackage(nuGetDependency.PackageName, nuGetDependency.PackageVersion, "Path/To/The.dll");
    }

    public NuGetPackage Dependency {
      get;
      private set;
    }
	}
}

