using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.Plugins.Dependencies {
	public class InternalDependency {
    public readonly Key Key;
    public readonly TaskKey ResolutionTask;

    public InternalDependency(Key key, TaskKey resolutionTask) {
      Key = key;
      ResolutionTask = resolutionTask;
    }

    public async Task<InternalDependency> Resolve(IContext context) {
      await context.Evaluate(ResolutionTask);
      return this;
    }
  }
}

