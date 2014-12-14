using Bud.Plugins.Dependencies;

namespace Bud.Plugins.NuGet {
  public class NuGetDependency : IDependency {
    public readonly string PackageName;
    public readonly string PackageVersion;

    public NuGetDependency(string packageName, string packageVersion) {
      this.PackageVersion = packageVersion;
      this.PackageName = packageName;
    }

    public System.Threading.Tasks.Task<IResolvedDependency> Resolve(EvaluationContext context) {
      throw new System.NotImplementedException();
    }
  }
}

