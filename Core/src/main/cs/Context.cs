using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bud.Logging;

namespace Bud {
  public interface IContext : IConfig {
    ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions { get; }
    bool IsTaskDefined(Key key);
    Task Evaluate(TaskKey key);
    Task<T> Evaluate<T>(TaskKey<T> key);
    Task EvaluateKey(Key key);
    Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition, Key taskKey);
    Task Evaluate(ITaskDefinition taskDefinition, Key taskKey);
  }

  public class Context : IContext {
    private readonly IConfig Configuration;
    private readonly ImmutableDictionary<Key, ITaskDefinition> taskDefinitions;
    private readonly Dictionary<Key, Task> TaskEvaluations = new Dictionary<Key, Task>();
    private readonly Dictionary<ITaskDefinition, Task> OverridenTaskEvaluations = new Dictionary<ITaskDefinition, Task>();
    private readonly SemaphoreSlim EvaluationStoreGuard = new SemaphoreSlim(1);

    private Context(ImmutableDictionary<Key, IConfigDefinition> configDefinitions, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions, ILogger logger) : this(new Config(configDefinitions, logger), taskDefinitions) {}

    private Context(IConfig configuration, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions) {
      this.Configuration = configuration;
      this.taskDefinitions = taskDefinitions;
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions => Configuration.ConfigDefinitions;

    public ILogger Logger => Configuration.Logger;

    public ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions => taskDefinitions;

    public bool IsConfigDefined(Key key) => Configuration.IsConfigDefined(key);

    public T Evaluate<T>(ConfigKey<T> configKey) => Configuration.Evaluate(configKey);

    public object EvaluateConfig(Key key) => Configuration.EvaluateConfig(key);

    public bool IsTaskDefined(Key key) => taskDefinitions.ContainsKey(key);

    public Task EvaluateKey(Key key) {
      var absoluteKey = Key.Root / key;
      if (IsTaskDefined(absoluteKey)) {
        return EvaluateTask(absoluteKey);
      }
      return Task.FromResult(Configuration.EvaluateConfig(absoluteKey));
    }

    public Task Evaluate(TaskKey key) => EvaluateTask(key);

    public Task<T> Evaluate<T>(TaskKey<T> key) => EvaluateTask<T>(key);

    public Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition, Key taskKey) => (Task<T>) Evaluate((ITaskDefinition) taskDefinition, taskKey);

    public Task Evaluate(ITaskDefinition taskDefinition, Key taskKey) {
      Task existingEvaluation;
      if (OverridenTaskEvaluations.TryGetValue(taskDefinition, out existingEvaluation)) {
        return existingEvaluation;
      }
      return EvaluateOverridenTaskToCacheUnguarded(taskDefinition, taskKey);
    }

    public static Context FromSettings(Settings settings, ILogger logger) {
      return new Context(settings.ConfigDefinitions, settings.TaskDefinitions, logger);
    }

    public static IContext FromSettings(Settings settings) {
      return FromSettings(settings, Logging.Logger.CreateFromStandardOutputs());
    }

    public static Context FromConfig(IConfig config, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions) {
      return new Context(config, taskDefinitions);
    }

    private Task EvaluateTask(Key key) {
      var absoluteKey = Key.Root / key;
      return TryGetTaskEvaluationFromCache(absoluteKey) ?? ExecuteGuarded(() => EvaluateTaskToCacheUnguarded(absoluteKey));
    }

    private Task<T> EvaluateTask<T>(Key key) {
      var absoluteKey = Key.Root / key;
      Task evaluation = TryGetTaskEvaluationFromCache(absoluteKey);
      return evaluation != null ? (Task<T>) evaluation : ExecuteGuarded(() => (Task<T>)EvaluateTaskToCacheUnguarded(absoluteKey));
    }

    private Task<T> ExecuteGuarded<T>(Func<Task<T>> actionToGuard) {
      return EvaluationStoreGuard
        .WaitAsync()
        .ContinueWith(t => actionToGuard())
        .ContinueWith(t => {
          EvaluationStoreGuard.Release();
          return t.Result;
        })
        .Unwrap();
    }

    private Task ExecuteGuarded(Func<Task> unguardedAction) {
      return EvaluationStoreGuard
        .WaitAsync()
        .ContinueWith(t => unguardedAction())
        .ContinueWith(t => {
          EvaluationStoreGuard.Release();
          return t.Result;
        })
        .Unwrap();
    }

    private Task TryGetTaskEvaluationFromCache(Key absoluteKey) {
      Task value;
      return TaskEvaluations.TryGetValue(absoluteKey, out value) ? value : null;
    }

    private Task EvaluateTaskToCacheUnguarded(Key absoluteKey) {
      ITaskDefinition taskDefinition;
      if (taskDefinitions.TryGetValue(absoluteKey, out taskDefinition)) {
        var value = taskDefinition.Evaluate(this, absoluteKey);
        TaskEvaluations.Add(absoluteKey, value);
        return value;
      }
      throw new ArgumentException(string.Format("Could not evaluate the task '{0}'. This task is not defined.", absoluteKey));
    }

    private Task EvaluateOverridenTaskToCacheUnguarded(ITaskDefinition taskDefinition, Key taskKey) {
      Task freshEvaluation = taskDefinition.Evaluate(this, taskKey);
      OverridenTaskEvaluations.Add(taskDefinition, freshEvaluation);
      return freshEvaluation;
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder("EvaluationContext(Configurations: [");
      ToString(sb, Configuration.ConfigDefinitions.Keys);
      sb.Append("], Tasks: [");
      ToString(sb, taskDefinitions.Keys);
      return sb.Append("])").ToString();
    }

    public static void ToString(StringBuilder sb, IEnumerable<Key> keys) {
      var enumerator = keys.GetEnumerator();
      if (enumerator.MoveNext()) {
        sb.Append(enumerator.Current);
        while (enumerator.MoveNext()) {
          sb.Append(", ");
          sb.Append(enumerator.Current);
        }
      }
    }
  }
}