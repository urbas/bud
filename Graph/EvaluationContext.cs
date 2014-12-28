using System;
using System.Collections.Immutable;
using System.Text;
using Bud.SettingsConstruction;
using Bud.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud {
  public class EvaluationContext {
    public readonly IDictionary<Scope, IValueDefinition> ScopesToValues;
    // TODO: Assign sequential indices to keys and use arrays to access evaluated values (instead of dictionaries).
    // TODO: Consider using concurrent dictionaries here (so that people can access the execution context and evaluate tasks from different threads).
    private readonly Dictionary<Scope, object> configValues = new Dictionary<Scope, object>();
    private readonly Dictionary<TaskKey, Task> taskValues = new Dictionary<TaskKey, Task>();
    private readonly Dictionary<ITaskDefinition, Task> modifiedTaskValues = new Dictionary<ITaskDefinition, Task>();
    private readonly EvaluationLogger evaluationLogger = new EvaluationLogger();
    private readonly Dictionary<Scope, string> scopeToOutput = new Dictionary<Scope, string>();

    private EvaluationContext(IDictionary<Scope, IValueDefinition> settingKeysToValues) {
      this.ScopesToValues = settingKeysToValues;
    }

    public bool Exists(Scope scope) {
      return ScopesToValues.ContainsKey(scope);
    }

    public Task Evaluate(Scope scope) {
      if (scope is TaskKey) {
        return Evaluate((TaskKey)scope);
      } else {
        return Task.FromResult(Evaluate<object>((ConfigKey<object>)scope));
      }
    }

    public T Evaluate<T>(ConfigKey<T> key) {
      object value;
      if (configValues.TryGetValue(key, out value)) {
        return (T)value;
      } else {
        T evaluatedValue = ((ConfigDefinition<T>)ScopesToValues[key]).Evaluate(this);
        configValues.Add(key, evaluatedValue);
        return evaluatedValue;
      }
    }

    public Task Evaluate(TaskKey key) {
      Task evaluationAsTask;
      if (!taskValues.TryGetValue(key, out evaluationAsTask)) {
        evaluationAsTask = ((ITaskDefinition)ScopesToValues[(Scope)key]).Evaluate(this);
        taskValues.Add(key, evaluationAsTask);
      }
      return evaluationAsTask;
    }

    public Task<T> Evaluate<T>(TaskKey<T> key) {
      return (Task<T>)Evaluate((TaskKey)key);
    }

    public Task<T> Evaluate<T>(TaskDefinition<T> overwrittenTaskDef) {
      Task existingEvaluation;
      if (modifiedTaskValues.TryGetValue(overwrittenTaskDef, out existingEvaluation)) {
        return (Task<T>)existingEvaluation;
      } else {
        Task<T> freshEvaluation = overwrittenTaskDef.Evaluate(this);
        modifiedTaskValues.Add(overwrittenTaskDef, freshEvaluation);
        return freshEvaluation;
      }
    }

    public string GetOutputOf(Scope scope) {
      return scopeToOutput[scope];
    }

    public static EvaluationContext FromSettings(Settings settings) {
      return new EvaluationContext(settings.Compile());
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder("EvaluationContext{");
      var enumerator = ScopesToValues.Keys.GetEnumerator();
      if (enumerator.MoveNext()) {
        sb.Append(enumerator.Current);
        while (enumerator.MoveNext()) {
          sb.Append(", ");
          sb.Append(enumerator.Current);
        }
      }
      return sb.Append('}').ToString();
    }
  }
}