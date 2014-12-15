using Bud.Plugins.Dependencies;
using System.Threading.Tasks;

namespace Bud.Plugins.NuGet {
  public class NuGetDependency : IDependency {
    public readonly string PackageName;
    public readonly string PackageVersion;

    public NuGetDependency(string packageName, string packageVersion) {
      this.PackageVersion = packageVersion;
      this.PackageName = packageName;
    }

    public Task<IResolvedDependency> Resolve(EvaluationContext context) {
      return (Task<IResolvedDependency>)context.GetNuGetDependencyResolver().Resolve(context, this);
    }
  }
}

