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
	public class NuGetPackage
	{
    public readonly string PackageName;
    public readonly string PackageVersion;
    public readonly string DllPath;

    public NuGetPackage(string packageName, string packageVersion, string dllPath) {
      this.DllPath = dllPath;
      this.PackageVersion = packageVersion;
      this.PackageName = packageName;
    }
	}
}

