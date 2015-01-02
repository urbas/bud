using System;
using System.Collections.Immutable;
using System.Text;
using Bud.SettingsConstruction;
using Bud.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud {

  public interface IEvaluationContext : IConfiguration {
    ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions { get; }
    bool IsTaskDefined(Key scope);
    Task<T> Evaluate<T>(TaskKey<T> scope);
    Task EvaluateTask(Key scope);
    Task EvaluateScope(Key scope);
  }

  // TODO: Make this class thread-safe.
  public class EvaluationContext : IEvaluationContext {
    private readonly IConfiguration configuration;
    private readonly ImmutableDictionary<Key, ITaskDefinition> taskDefinitions;
    private readonly Dictionary<Key, Task> taskValues = new Dictionary<Key, Task>();
    private readonly Dictionary<ITaskDefinition, Task> oldTaskValues = new Dictionary<ITaskDefinition, Task>();
    private readonly Dictionary<Key, object> scopeToOutput = new Dictionary<Key, object>();

    private EvaluationContext(ImmutableDictionary<Key, IConfigDefinition> configDefinitions, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions) : this(new Configuration(configDefinitions), taskDefinitions) {}

    private EvaluationContext(IConfiguration configuration, ImmutableDictionary<Key, ITaskDefinition> taskDefinitions) {
      this.configuration = configuration;
      this.taskDefinitions = taskDefinitions;
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions { get { return configuration.ConfigDefinitions; } }

    public ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions { get { return taskDefinitions; } }

    public bool IsConfigDefined(Key scope) {
      return configuration.IsConfigDefined(scope);
    }

    public T Evaluate<T>(ConfigKey<T> configKey) {
      return configuration.Evaluate(configKey);
    }

    public object EvaluateConfig(Key scope) {
      return configuration.EvaluateConfig(scope);
    }

    public bool IsTaskDefined(Key scope) {
      return taskDefinitions.ContainsKey(scope);
    }

    public Task EvaluateScope(Key scope) {
      if (IsTaskDefined(scope)) {
        return EvaluateTask(scope);
      }
      return Task.FromResult(configuration.EvaluateConfig(scope));
    }

    public Task Evaluate(TaskKey scope) {
      return EvaluateTask(scope);
    }

    public Task<T> Evaluate<T>(TaskKey<T> scope) {
      return (Task<T>)Evaluate((TaskKey)scope);
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

    public static EvaluationContext FromSettings(Settings settings) {
      return new EvaluationContext(settings.ConfigDefinitions, settings.TaskDefinitions);
    }

    public Task EvaluateTask(Key scope) {
      Task value;
      if (taskValues.TryGetValue(scope, out value)) {
        return value;
      }
      ITaskDefinition taskDefinition;
      if (taskDefinitions.TryGetValue(scope, out taskDefinition)) {
        value = taskDefinition.Evaluate(this);
        taskValues.Add(scope, value);
        return value;
      }
      throw new ArgumentException(string.Format("Could not evaluate the task '{0}'. The value for this task was not defined.", scope));
    }

    public object GetOutputOf(Key scope) {
      object evaluationOutput;
      if (scopeToOutput.TryGetValue(scope, out evaluationOutput)) {
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

    public static void ToString(StringBuilder sb, IEnumerable<Key> scopes) {
      var enumerator = scopes.GetEnumerator();
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