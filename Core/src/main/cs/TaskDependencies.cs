using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Bud {
  public abstract class TaskDependencies {
    public ImmutableHashSet<TaskKey> Dependencies { get; private set; }

    protected TaskDependencies(ImmutableHashSet<TaskKey> dependencies) {
      Dependencies = dependencies;
    }

    protected async Task InvokeDependencies(IContext context) {
      foreach (var dependency in Dependencies) {
        await context.Evaluate(dependency);
      }
    }
  }
}