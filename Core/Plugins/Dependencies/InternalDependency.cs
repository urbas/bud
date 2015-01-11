using System.Threading.Tasks;

namespace Bud.Plugins.Deps {
  public class InternalDependency {
    public readonly Key DepdendencyTarget;
    public readonly TaskKey ResolutionTask;

    public InternalDependency(Key depdendencyTarget, TaskKey resolutionTask) {
      DepdendencyTarget = depdendencyTarget;
      ResolutionTask = resolutionTask;
    }

    public async Task Resolve(IContext context) {
      await context.Evaluate(ResolutionTask);
    }
  }
}