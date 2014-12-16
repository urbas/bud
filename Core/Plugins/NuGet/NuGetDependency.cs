using Bud.Plugins.Dependencies;
using System.Threading.Tasks;
using NuGet.Versioning;

namespace Bud.Plugins.NuGet {
  public class NuGetDependency : IDependency {
    public readonly string PackageName;
    public readonly NuGetVersion PackageVersion;

    public NuGetDependency(string packageName, string packageVersion) {
      this.PackageName = packageName;
      this.PackageVersion = global::NuGet.Versioning.NuGetVersion.Parse(packageVersion);
    }

    public Task<IResolvedDependency> Resolve(EvaluationContext context) {
      return context.GetNuGetDependencyResolver().Resolve(context, this);
    }
  }
}

