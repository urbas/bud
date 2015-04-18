using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bud.Logging;
using Bud.Util;

namespace Bud {
  public class Context : IContext {
    private readonly IConfig Configuration;
    private ImmutableDictionary<Key, Task> TaskValueCache = ImmutableDictionary<Key, Task>.Empty;
    private readonly SemaphoreSlim TaskValueCacheSemaphore = new SemaphoreSlim(1);
    private ImmutableDictionary<ITaskDefinition, Task> OverridenTaskValueCache = ImmutableDictionary<ITaskDefinition, Task>.Empty;
    private readonly SemaphoreSlim OverridenTaskValueCacheSemaphore = new SemaphoreSlim(1);

    private Context(ImmutableDictionary<ConfigKey, IConfigDefinition> configDefinitions, ImmutableDictionary<TaskKey, ITaskDefinition> taskDefinitions, ILogger logger) : this(new Config(configDefinitions, logger), taskDefinitions) {}

    private Context(IConfig configuration, ImmutableDictionary<TaskKey, ITaskDefinition> taskDefinitions) {
      Configuration = configuration;
      TaskDefinitions = taskDefinitions;
    }

    public ImmutableDictionary<ConfigKey, IConfigDefinition> ConfigDefinitions => Configuration.ConfigDefinitions;

    public ILogger Logger => Configuration.Logger;

    public ImmutableDictionary<TaskKey, ITaskDefinition> TaskDefinitions { get; }

    public bool IsConfigDefined(Key key) => Configuration.IsConfigDefined(key);

    public T Evaluate<T>(ConfigKey<T> configKey) => Configuration.Evaluate(configKey);

    public object EvaluateConfig(Key key) => Configuration.EvaluateConfig(key);

    public bool IsTaskDefined(Key key) => TaskDefinitions.ContainsKey(key);

    public Task EvaluateKey(Key key) {
      var absoluteKey = Key.Root / key;
      if (IsTaskDefined(absoluteKey)) {
        return EvaluateTask(absoluteKey);
      }
      return Task.FromResult(Configuration.EvaluateConfig(absoluteKey));
    }

    public Task Evaluate(TaskKey key) => EvaluateTask(key);

    public Task<T> Evaluate<T>(TaskKey<T> key) => EvaluateTask<T>(key);

    public Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition, Key taskKey) {
      Task existingEvaluation;
      if (OverridenTaskValueCache.TryGetValue(taskDefinition, out existingEvaluation)) {
        return (Task<T>) existingEvaluation;
      }
      return TaskUtils.ExecuteGuarded(OverridenTaskValueCacheSemaphore, () => (Task<T>) EvaluateOverridenTaskToCacheUnguarded(taskDefinition, taskKey));
    }

    public Task Evaluate(ITaskDefinition taskDefinition, Key taskKey) {
      Task existingValue;
      if (OverridenTaskValueCache.TryGetValue(taskDefinition, out existingValue)) {
        return existingValue;
      }
      return TaskUtils.ExecuteGuarded(OverridenTaskValueCacheSemaphore, () => EvaluateOverridenTaskToCacheUnguarded(taskDefinition, taskKey));
    }

    public static Context FromSettings(Settings settings, ILogger logger) {
      return new Context(settings.ConfigDefinitions, settings.TaskDefinitions, logger);
    }

    public static IContext FromSettings(Settings settings) {
      return FromSettings(settings, Logging.Logger.CreateFromStandardOutputs());
    }

    public static Context FromConfig(IConfig config, ImmutableDictionary<TaskKey, ITaskDefinition> taskDefinitions) {
      return new Context(config, taskDefinitions);
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder("EvaluationContext(Configurations: [");
      ToString(sb, Configuration.ConfigDefinitions.Keys);
      sb.Append("], Tasks: [");
      ToString(sb, TaskDefinitions.Keys);
      return sb.Append("])").ToString();
    }

    public static void ToString(StringBuilder sb, IEnumerable<IKey> keys) {
      var enumerator = keys.GetEnumerator();
      if (enumerator.MoveNext()) {
        sb.Append(enumerator.Current);
        while (enumerator.MoveNext()) {
          sb.Append(", ");
          sb.Append(enumerator.Current);
        }
      }
    }

    private Task EvaluateTask(Key key) {
      var absoluteKey = Key.Root / key;
      return TryGetTaskEvaluationFromCache(absoluteKey) ?? TaskUtils.ExecuteGuarded(TaskValueCacheSemaphore, () => EvaluateTaskToCacheUnguarded(absoluteKey));
    }

    private Task<T> EvaluateTask<T>(Key key) {
      var absoluteKey = Key.Root / key;
      return (Task<T>) (TryGetTaskEvaluationFromCache(absoluteKey) ?? TaskUtils.ExecuteGuarded(TaskValueCacheSemaphore, () => (Task<T>) EvaluateTaskToCacheUnguarded(absoluteKey)));
    }

    private Task TryGetTaskEvaluationFromCache(Key absoluteKey) {
      Task value;
      return TaskValueCache.TryGetValue(absoluteKey, out value) ? value : null;
    }

    private Task EvaluateTaskToCacheUnguarded(Key absoluteKey) {
      ITaskDefinition taskDefinition;
      if (TaskDefinitions.TryGetValue(absoluteKey, out taskDefinition)) {
        var value = EvaluateTaskDefinition(absoluteKey, taskDefinition);
        TaskValueCache = TaskValueCache.Add(absoluteKey, value);
        return value;
      }
      throw new ArgumentException(string.Format("Could not evaluate the task '{0}'. This task is not defined.", absoluteKey));
    }

    private Task EvaluateOverridenTaskToCacheUnguarded(ITaskDefinition taskDefinition, Key taskKey) {
      Task freshEvaluation = EvaluateTaskDefinition(taskKey, taskDefinition);
      OverridenTaskValueCache = OverridenTaskValueCache.Add(taskDefinition, freshEvaluation);
      return freshEvaluation;
    }

    private Task EvaluateTaskDefinition(Key absoluteKey, ITaskDefinition taskDefinition) {
      return taskDefinition.Evaluate(new ScopedContext(this, absoluteKey), absoluteKey);
    }
  }
}