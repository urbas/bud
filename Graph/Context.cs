using System;
using System.Collections.Immutable;
using System.Text;
using Bud.SettingsConstruction;
using Bud.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud {

  public interface IContext : IConfig {
    ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions { get; }
    bool IsTaskDefined(Key key);
    Task Evaluate(TaskKey key);
    Task<T> Evaluate<T>(TaskKey<T> key);
    Task EvaluateTask(Key key);
    Task EvaluateKey(Key key);
    Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition);
  }

  // TODO: Make this class thread-safe.
  public class Context : IContext {
    private readonly IConfig configuration;
    private readonly ImmutableDictionary<Key, ITaskDefinition> taskDefinitions;
    private readonly Dictionary<Key, Task> taskValues = new Dictionary<Key, Task>();
    private readonly Dictionary<ITaskDefinition, Task> oldTaskValues = new Dictionary<ITaskDefinition, Task>();
    private readonly Dictionary<Key, object> keyToOutput = new Dictionary<Key, object>();

    private Context(ImmutableDictionary<Key, IConfigDefinition> configDefinitions, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions) : this(new Config(configDefinitions), taskDefinitions) {}

    private Context(IConfig configuration, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions) {
      this.configuration = configuration;
      this.taskDefinitions = taskDefinitions;
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions { get { return configuration.ConfigDefinitions; } }

    public ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions { get { return taskDefinitions; } }

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
      if (IsTaskDefined(key)) {
        return EvaluateTask(key);
      }
      return Task.FromResult(configuration.EvaluateConfig(key));
    }

    public Task Evaluate(TaskKey key) {
      return EvaluateTask(key);
    }

    public Task<T> Evaluate<T>(TaskKey<T> key) {
      return (Task<T>)Evaluate((TaskKey)key);
    }

    public Task<T> Evaluate<T>(TaskDefinition<T> taskDefinition) {
      Task existingEvaluation;
      if (oldTaskValues.TryGetValue(taskDefinition, out existingEvaluation)) {
        return (Task<T>)existingEvaluation;
      } else {
        Task<T> freshEvaluation = taskDefinition.Evaluate(this);
        oldTaskValues.Add(taskDefinition, freshEvaluation);
        return freshEvaluation;
      }
    }

    public static Context FromSettings(Settings settings) {
      return new Context(settings.ConfigDefinitions, settings.TaskDefinitions);
    }

    public Task EvaluateTask(Key key) {
      Task value;
      if (taskValues.TryGetValue(key, out value)) {
        return value;
      }
      ITaskDefinition taskDefinition;
      if (taskDefinitions.TryGetValue(key, out taskDefinition)) {
        value = taskDefinition.Evaluate(this);
        taskValues.Add(key, value);
        return value;
      }
      throw new ArgumentException(string.Format("Could not evaluate the task '{0}'. The value for this task was not defined.", key));
    }

    public object GetOutputOf(Key key) {
      object evaluationOutput;
      if (keyToOutput.TryGetValue(key, out evaluationOutput)) {
        return evaluationOutput;
      }
      return null;
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