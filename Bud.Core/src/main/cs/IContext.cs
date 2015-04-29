using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.SettingsConstruction;

namespace Bud {
  public interface IContext : IConfig {
    ImmutableDictionary<TaskKey, ITaskDefinition> TaskDefinitions { get; }
    bool IsTaskDefined(Key key);
    Task Evaluate(TaskKey key);
    Task<T> Evaluate<T>(TaskKey<T> key);
    object EvaluateKeySync(Key key);
    Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition, Key taskKey);
    Task Evaluate(ITaskDefinition taskDefinition, Key taskKey);
  }
}