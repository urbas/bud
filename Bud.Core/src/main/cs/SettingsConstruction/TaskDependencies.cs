using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Bud.SettingsConstruction {
  public abstract class TaskDependencies {
    public ImmutableHashSet<TaskKey> Dependencies { get; }

    protected TaskDependencies(ImmutableHashSet<TaskKey> dependencies) {
      Dependencies = dependencies;
    }

    protected async Task InvokeDependencies(IContext context) {
      await Task.WhenAll(Dependencies.Select(context.Evaluate));
    }
  }
}