using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.Tasking {
  public class TaskDefinitions {
    public static readonly TaskDefinitions Empty = new TaskDefinitions(ImmutableDictionary<string, ITaskDefinition>.Empty);
    public ImmutableDictionary<string, ITaskDefinition> Definitions { get; }

    private TaskDefinitions(ImmutableDictionary<string, ITaskDefinition> definitions) {
      Definitions = definitions;
    }

    public TaskDefinitions SetAsync<T>(string taskName, Func<IContext, Task<T>> task) {
      ITaskDefinition previousTaskDefinition;
      if (Definitions.TryGetValue(taskName, out previousTaskDefinition)) {
        if (previousTaskDefinition.ReturnType != typeof(T)) {
          throw new TaskTypeOverrideException($"Could not redefine the type of task '{taskName}' from '{previousTaskDefinition.ReturnType}' to '{typeof(T)}'. Redefinition of task types is not allowed.");
        }
      }
      return new TaskDefinitions(Definitions.SetItem(taskName, new TaskDefinition<T>(taskName, task)));
    }

    public TaskDefinitions ModifyAsync<T>(string taskName, Func<IContext, Task<T>, Task<T>> task) {
      ITaskDefinition previousTaskDefinition;
      if (Definitions.TryGetValue(taskName, out previousTaskDefinition)) {
        if (previousTaskDefinition.ReturnType != typeof(T)) {
          throw new TaskTypeOverrideException($"Could not redefine the type of task '{taskName}' from '{previousTaskDefinition.ReturnType}' to '{typeof(T)}'. Redefinition of task types is not allowed.");
        }
        return new TaskDefinitions(Definitions.SetItem(taskName, new TaskDefinition<T>(taskName, ((TaskDefinition<T>) previousTaskDefinition).Task, task)));
      }
      throw new TaskUndefinedException($"Could not modify the task '{taskName}'. The task is not defined yet.");
    }
  }
}