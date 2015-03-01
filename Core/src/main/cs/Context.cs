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
    private ImmutableDictionary<Key, Task> TaskEvaluationCache = ImmutableDictionary<Key, Task>.Empty;
    private readonly SemaphoreSlim EvaluationCacheSemaphore = new SemaphoreSlim(1);
    private ImmutableDictionary<ITaskDefinition, Task> OverridenTaskEvaluations = ImmutableDictionary<ITaskDefinition, Task>.Empty;
    private readonly SemaphoreSlim OverridenTaskEvaluationCacheSemaphore = new SemaphoreSlim(1);

    private Context(ImmutableDictionary<Key, IConfigDefinition> configDefinitions, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions, ILogger logger) : this(new Config(configDefinitions, logger), taskDefinitions) {}

    private Context(IConfig configuration, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions) {
      Configuration = configuration;
      TaskDefinitions = taskDefinitions;
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions => Configuration.ConfigDefinitions;

    public ILogger Logger => Configuration.Logger;

    public ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions { get; }

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
      return OverridenTaskEvaluations.TryGetValue(taskDefinition, out existingEvaluation) ? (Task<T>) existingEvaluation : TaskUtils.ExecuteGuarded(OverridenTaskEvaluationCacheSemaphore, () => (Task<T>) EvaluateOverridenTaskToCacheUnguarded(taskDefinition, taskKey));
    }

    public Task Evaluate(ITaskDefinition taskDefinition, Key taskKey) {
      Task existingEvaluation;
      return OverridenTaskEvaluations.TryGetValue(taskDefinition, out existingEvaluation) ? existingEvaluation : TaskUtils.ExecuteGuarded(OverridenTaskEvaluationCacheSemaphore, () => EvaluateOverridenTaskToCacheUnguarded(taskDefinition, taskKey));
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
      return TryGetTaskEvaluationFromCache(absoluteKey) ?? TaskUtils.ExecuteGuarded(EvaluationCacheSemaphore, () => EvaluateTaskToCacheUnguarded(absoluteKey));
    }

    private Task<T> EvaluateTask<T>(Key key) {
      var absoluteKey = Key.Root / key;
      return (Task<T>) (TryGetTaskEvaluationFromCache(absoluteKey) ?? TaskUtils.ExecuteGuarded(EvaluationCacheSemaphore, () => (Task<T>) EvaluateTaskToCacheUnguarded(absoluteKey)));
    }

    private Task TryGetTaskEvaluationFromCache(Key absoluteKey) {
      Task value;
      return TaskEvaluationCache.TryGetValue(absoluteKey, out value) ? value : null;
    }

    private Task EvaluateTaskToCacheUnguarded(Key absoluteKey) {
      ITaskDefinition taskDefinition;
      if (TaskDefinitions.TryGetValue(absoluteKey, out taskDefinition)) {
        var value = EvaluateTaskDefinition(absoluteKey, taskDefinition);
        TaskEvaluationCache = TaskEvaluationCache.Add(absoluteKey, value);
        return value;
      }
      throw new ArgumentException(string.Format("Could not evaluate the task '{0}'. This task is not defined.", absoluteKey));
    }

    private Task EvaluateOverridenTaskToCacheUnguarded(ITaskDefinition taskDefinition, Key taskKey) {
      Task freshEvaluation = EvaluateTaskDefinition(taskKey, taskDefinition);
      OverridenTaskEvaluations = OverridenTaskEvaluations.Add(taskDefinition, freshEvaluation);
      return freshEvaluation;
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder("EvaluationContext(Configurations: [");
      ToString(sb, Configuration.ConfigDefinitions.Keys);
      sb.Append("], Tasks: [");
      ToString(sb, TaskDefinitions.Keys);
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

    private Task EvaluateTaskDefinition(Key absoluteKey, ITaskDefinition taskDefinition) {
      return taskDefinition.Evaluate(new ScopedContext(this, absoluteKey), absoluteKey);
    }
  }
}