using Bud.Plugins.Dependencies;
using System.Threading.Tasks;
using NuGet;

namespace Bud.Plugins.NuGet {
  public class NuGetDependency {
    public readonly string PackageName;
    public readonly SemanticVersion PackageVersion;

    public NuGetDependency(string packageName, string packageVersion) {
      this.PackageName = packageName;
      this.PackageVersion = SemanticVersion.Parse(packageVersion);
    }
  }
}

