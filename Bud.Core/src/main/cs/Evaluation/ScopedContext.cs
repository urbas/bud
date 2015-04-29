using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Logging;
using Bud.SettingsConstruction;

namespace Bud.Evaluation {
  internal class ScopedContext : IContext {
    private readonly Context Context;

    public ScopedContext(Context context, Key taskKey) {
      Context = context;
      Logger = new ScopedLogger(context.Logger, taskKey);
    }

    public ImmutableDictionary<ConfigKey, IConfigDefinition> ConfigDefinitions => Context.ConfigDefinitions;

    public ILogger Logger { get; }

    public bool IsConfigDefined(Key key) {
      return Context.IsConfigDefined(key);
    }

    public Task Evaluate(Key key) => Context.Evaluate(key);

    public T Evaluate<T>(ConfigKey<T> configKey) => Context.Evaluate(configKey);

    public bool TryEvaluate<T>(ConfigKey<T> configKey, out T evaluatedValue) => Context.TryEvaluate(configKey, out evaluatedValue);

    public object EvaluateConfig(Key key) => Context.EvaluateConfig(key);

    public ImmutableDictionary<TaskKey, ITaskDefinition> TaskDefinitions => Context.TaskDefinitions;

    public bool IsTaskDefined(Key key) => Context.IsTaskDefined(key);

    public Task Evaluate(TaskKey key) => Context.Evaluate(key);

    public Task<T> Evaluate<T>(TaskKey<T> key) => Context.Evaluate(key);

    public object EvaluateKeySync(Key key) => Context.EvaluateKeySync(key);

    public Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition, Key taskKey) => Context.Evaluate(taskDefinition, taskKey);

    public Task Evaluate(ITaskDefinition taskDefinition, Key taskKey) => Context.Evaluate(taskDefinition, taskKey);
  }
}