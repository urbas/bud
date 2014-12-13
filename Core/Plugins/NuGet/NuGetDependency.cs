using Bud.Plugins.Dependencies;

namespace Bud.Plugins.NuGet {
  public class NuGetDependency : IDependency {
    public NuGetDependency(string packageName, string packageVersion) {
    }

    public System.Threading.Tasks.Task<IResolvedDependency> Resolve(EvaluationContext context) {
      throw new System.NotImplementedException();
    }
  }
}

