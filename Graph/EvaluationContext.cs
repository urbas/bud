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
    private readonly Dictionary<Scope, Task> taskValues = new Dictionary<Scope, Task>();
    private readonly Dictionary<ITaskDefinition, Task> modifiedTaskValues = new Dictionary<ITaskDefinition, Task>();
    private readonly Dictionary<Scope, object> scopeToOutput = new Dictionary<Scope, object>();

    private EvaluationContext(IDictionary<Scope, IValueDefinition> settingKeysToValues) {
      this.ScopesToValues = settingKeysToValues;
    }

    public bool Exists(Scope scope) {
      return ScopesToValues.ContainsKey(scope);
    }

    public Task Evaluate(Scope scope) {
      Task task;
      if (taskValues.TryGetValue(scope, out task)) {
        return task;
      }

      object value;
      if (configValues.TryGetValue(scope, out value)) {
        return Task.FromResult(value);
      }

      if (TryEvaluateAndCacheValue(scope, out value)) {
        return (value as Task) ?? Task.FromResult(value);
      }

      throw new ArgumentException(string.Format("Could not evaluate scope '{0}'. The value for this scope was not defined.", scope));
    }

    private bool TryEvaluateAndCacheValue(Scope taskScope, out object value) {
      IValueDefinition valueDefinition;
      if (ScopesToValues.TryGetValue(taskScope, out valueDefinition)) {
        if (valueDefinition is ITaskDefinition) {
          Task taskValue = ((ITaskDefinition)valueDefinition).Evaluate(this);
          value = taskValue;
          taskValues.Add(taskScope, taskValue);
        } else {
          value = valueDefinition.Evaluate(this);
          configValues.Add(taskScope, value);
        }
        return true;
      }
      value = null;
      return false;
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

    public object GetOutputOf(Scope scope) {
      object evaluationOutput;
      if (scopeToOutput.TryGetValue(scope, out evaluationOutput)) {
        return evaluationOutput;
      }
      return null;
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