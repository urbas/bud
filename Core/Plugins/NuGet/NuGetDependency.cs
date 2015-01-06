using Bud.Plugins.Dependencies;
using System.Threading.Tasks;
using NuGet;

namespace Bud.Plugins.NuGet {
  public class NuGetDependency {
    public readonly string PackageId;
    public readonly SemanticVersion PackageVersion;

    public NuGetDependency(string packageId, string packageVersion = null) {
      this.PackageId = packageId;
      this.PackageVersion = packageVersion == null ? null : SemanticVersion.Parse(packageVersion);
    }
  }
}

