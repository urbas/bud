using System.Threading.Tasks;
using NuGet;

namespace Bud.Dependencies {
  public abstract class InternalDependency : IDependency {
    public readonly Key DependencyTarget;
    public readonly TaskKey ResolutionTask;

    public InternalDependency(Key dependencyTarget, TaskKey resolutionTask) {
      DependencyTarget = dependencyTarget;
      ResolutionTask = resolutionTask;
    }

    public async Task Resolve(IContext context) {
      await context.Evaluate(ResolutionTask);
    }

    public abstract IPackage AsPackage(IConfig config);
  }
}