using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;
using Bud.Commander;
using System.Threading.Tasks;
using Bud.Plugins.Dependencies;
using NuGet.Versioning;

namespace Bud.Plugins.NuGet
{
	public class NuGetPackage
	{
    public readonly string PackageName;
    public readonly NuGetVersion PackageVersion;
    public readonly string DllPath;

    public NuGetPackage(string packageName, NuGetVersion packageVersion, string dllPath) {
      this.DllPath = dllPath;
      this.PackageVersion = packageVersion;
      this.PackageName = packageName;
    }
	}
}

