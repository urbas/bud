using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Bud.Logging;

namespace Bud {
  public interface IContext : IConfig {
    ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions { get; }
    bool IsTaskDefined(Key key);
    Task Evaluate(TaskKey key);
    Task<T> Evaluate<T>(TaskKey<T> key);
    Task EvaluateTask(Key key);
    Task EvaluateKey(Key key);
    Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition);
    Task Evaluate(ITaskDefinition taskDefinition);
  }

  // TODO: Make this class thread-safe.
  public class Context : IContext {
    private readonly IConfig configuration;
    private readonly ImmutableDictionary<Key, ITaskDefinition> taskDefinitions;
    private readonly Dictionary<Key, Task> taskValues = new Dictionary<Key, Task>();

    private readonly Dictionary<ITaskDefinition, Task> oldTaskValues = new Dictionary<ITaskDefinition, Task>();

    private Context(ImmutableDictionary<Key, IConfigDefinition> configDefinitions, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions, ILogger logger) : this(new Config(configDefinitions, logger), taskDefinitions) {}

    private Context(IConfig configuration, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions) {
      this.configuration = configuration;
      this.taskDefinitions = taskDefinitions;
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions {
      get { return configuration.ConfigDefinitions; }
    }

    public ILogger Logger {
      get { return configuration.Logger; }
    }

    public ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions {
      get { return taskDefinitions; }
    }

    public bool IsConfigDefined(Key key) {
      return configuration.IsConfigDefined(key);
    }

    public T Evaluate<T>(ConfigKey<T> configKey) {
      return configuration.Evaluate(configKey);
    }

    public object EvaluateConfig(Key key) {
      return configuration.EvaluateConfig(key);
    }

    public bool IsTaskDefined(Key key) {
      return taskDefinitions.ContainsKey(key);
    }

    public Task EvaluateKey(Key key) {
      var absoluteKey = key.In(Key.Root);
      if (IsTaskDefined(absoluteKey)) {
        return EvaluateTask(absoluteKey);
      }
      return Task.FromResult(configuration.EvaluateConfig(absoluteKey));
    }

    public Task Evaluate(TaskKey key) {
      return EvaluateTask(key);
    }

    public Task<T> Evaluate<T>(TaskKey<T> key) {
      return (Task<T>) Evaluate((TaskKey) key);
    }

    public Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition) {
      return (Task<T>) Evaluate((ITaskDefinition) taskDefinition);
    }

    public Task Evaluate(ITaskDefinition taskDefinition) {
      Task existingEvaluation;
      if (oldTaskValues.TryGetValue(taskDefinition, out existingEvaluation)) {
        return existingEvaluation;
      }
      Task freshEvaluation = taskDefinition.Evaluate(this);
      oldTaskValues.Add(taskDefinition, freshEvaluation);
      return freshEvaluation;
    }

    public static Context FromSettings(Settings settings, ILogger logger) {
      return new Context(settings.ConfigDefinitions, settings.TaskDefinitions, logger);
    }

    public static IContext FromSettings(Settings settings)
    {
      return FromSettings(settings, Logging.Logger.CreateFromStandardOutputs());
    }

    public static Context FromConfig(IConfig config, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions) {
      return new Context(config, taskDefinitions);
    }

    public Task EvaluateTask(Key key) {
      Task value;
      var absoluteKey = key.In(Key.Root);
      if (taskValues.TryGetValue(absoluteKey, out value)) {
        return value;
      }
      ITaskDefinition taskDefinition;
      if (taskDefinitions.TryGetValue(absoluteKey, out taskDefinition)) {
        value = taskDefinition.Evaluate(this);
        taskValues.Add(absoluteKey, value);
        return value;
      }
      throw new ArgumentException(string.Format("Could not evaluate the task '{0}'. The value for this task was not defined.", absoluteKey));
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder("EvaluationContext(Configurations: [");
      ToString(sb, configuration.ConfigDefinitions.Keys);
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