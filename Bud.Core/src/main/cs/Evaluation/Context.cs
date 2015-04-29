using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bud.Logging;
using Bud.SettingsConstruction;

namespace Bud.Evaluation {
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

    public Task Evaluate(Key key) {
      object value;
      return Configuration.TryEvaluate(key, out value) ? Task.FromResult(value) : Evaluate((TaskKey)key);
    }

    public T Evaluate<T>(ConfigKey<T> configKey) => Configuration.Evaluate(configKey);

    public bool TryEvaluate<T>(ConfigKey<T> configKey, out T evaluatedValue) => Configuration.TryEvaluate(configKey, out evaluatedValue);

    public object EvaluateConfig(Key key) => Configuration.EvaluateConfig(key);

    public bool IsTaskDefined(Key key) => TaskDefinitions.ContainsKey(key);

    public object EvaluateKeySync(Key key) {
      var absoluteKey = Key.Root / key;
      object value;
      return TryEvaluateTaskSync(absoluteKey, out value) ? value : Configuration.EvaluateConfig(absoluteKey);
    }

    public Task Evaluate(TaskKey key) => EvaluateTask(key);

    public Task<T> Evaluate<T>(TaskKey<T> key) => EvaluateTask<T>(key);

    public Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition, Key taskKey) {
      return (Task<T>) (TryGetOverridenTaskValueFromCache(taskDefinition) ?? EvaluateOverridenTaskToCache(taskDefinition, taskKey));
    }

    public Task Evaluate(ITaskDefinition taskDefinition, Key taskKey) {
      return TryGetOverridenTaskValueFromCache(taskDefinition) ??
             EvaluateOverridenTaskToCache(taskDefinition, taskKey);
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
      return TryGetTaskValueFromCache(absoluteKey) ?? EvaluateTaskToCache(absoluteKey);
    }

    private bool TryEvaluateTaskSync(Key absoluteKey, out object value) {
      ITaskDefinition taskDefinition;
      if (TaskDefinitions.TryGetValue(absoluteKey, out taskDefinition)) {
        var valueTask = EvaluateTask(absoluteKey);
        valueTask.Wait();
        value = taskDefinition.ExtractResult(valueTask);
        return true;
      }
      value = null;
      return false;
    }

    private Task<T> EvaluateTask<T>(Key key) {
      var absoluteKey = Key.Root / key;
      return (Task<T>) (TryGetTaskValueFromCache(absoluteKey) ?? EvaluateTaskToCache(absoluteKey));
    }

    private Task TryGetTaskValueFromCache(Key absoluteKey) {
      Task value;
      return TaskValueCache.TryGetValue(absoluteKey, out value) ? value : null;
    }

    private Task TryGetOverridenTaskValueFromCache(ITaskDefinition taskDefinition) {
      Task existingValue;
      OverridenTaskValueCache.TryGetValue(taskDefinition, out existingValue);
      return existingValue;
    }

    private Task EvaluateTaskToCache(Key absoluteKey) {
      ITaskDefinition taskDefinition;
      if (TaskDefinitions.TryGetValue(absoluteKey, out taskDefinition)) {
        return taskDefinition.EvaluateGuarded(new ScopedContext(this, absoluteKey),
                                              absoluteKey,
                                              TaskValueCacheSemaphore,
                                              () => TryGetTaskValueFromCache(absoluteKey),
                                              valueTask => TaskValueCache = TaskValueCache.Add(absoluteKey, valueTask));
      }
      throw new ArgumentException(Config.KeyUndefinedEvaluationFailedMessage(absoluteKey));
    }

    private Task EvaluateOverridenTaskToCache(ITaskDefinition taskDefinition, Key taskKey) {
      return taskDefinition.EvaluateGuarded(new ScopedContext(this, taskKey),
                                            taskKey,
                                            OverridenTaskValueCacheSemaphore,
                                            () => TryGetOverridenTaskValueFromCache(taskDefinition),
                                            valueTask => OverridenTaskValueCache = OverridenTaskValueCache.Add(taskDefinition, valueTask));
    }
  }
}