using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Logging;

namespace Bud {
  internal class ScopedContext : IContext {
    private readonly Context Context;

    public ScopedContext(Context context, Key taskKey) {
      Context = context;
      Logger = new ScopedLogger(context.Logger, taskKey);
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions => Context.ConfigDefinitions;

    public ILogger Logger { get; }

    public bool IsConfigDefined(Key key) {
      return ((IConfig)Context).IsConfigDefined(key);
    }

    public T Evaluate<T>(ConfigKey<T> configKey) {
      return ((IConfig)Context).Evaluate<T>(configKey);
    }

    public object EvaluateConfig(Key key) {
      return ((IConfig)Context).EvaluateConfig(key);
    }

    public ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions => Context.TaskDefinitions;

    public bool IsTaskDefined(Key key) {
      return Context.IsTaskDefined(key);
    }

    public Task Evaluate(TaskKey key) {
      return Context.Evaluate(key);
    }

    public Task<T> Evaluate<T>(TaskKey<T> key) {
      return Context.Evaluate(key);
    }

    public Task EvaluateKey(Key key) {
      return Context.EvaluateKey(key);
    }

    public Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition, Key taskKey) {
      return Context.Evaluate(taskDefinition, taskKey);
    }

    public Task Evaluate(ITaskDefinition taskDefinition, Key taskKey) {
      return Context.Evaluate(taskDefinition, taskKey);
    }
  }
}