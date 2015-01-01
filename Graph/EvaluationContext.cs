using System;
using System.Collections.Immutable;
using System.Text;
using Bud.SettingsConstruction;
using Bud.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud {

  // TODO: Make this class thread-safe.
  public class EvaluationContext : Configuration {
    private readonly Dictionary<Scope, Task> taskValues = new Dictionary<Scope, Task>();
    private readonly Dictionary<ITaskDefinition, Task> oldTaskValues = new Dictionary<ITaskDefinition, Task>();
    private readonly Dictionary<Scope, object> scopeToOutput = new Dictionary<Scope, object>();

    public EvaluationContext(ScopeDefinitions scopeDefinitions) : base(scopeDefinitions) {
    }

    public Task EvaluateScope(Scope scope) {
      if (IsTaskDefined(scope)) {
        return EvaluateTask(scope);
      }
      return Task.FromResult(EvaluateConfig(scope));
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
      return new EvaluationContext(settings.Compile());
    }

    protected Task EvaluateTask(Scope scope) {
      Task value;
      if (taskValues.TryGetValue(scope, out value)) {
        return value;
      }
      ITaskDefinition taskDefinition;
      if (ScopeDefinitions.TaskDefinitions.TryGetValue(scope, out taskDefinition)) {
        value = taskDefinition.Evaluate(this);
        taskValues.Add(scope, value);
        return value;
      }
      throw new ArgumentException(string.Format("Could not evaluate the task '{0}'. The value for this task was not defined.", scope));
    }

    public object GetOutputOf(Scope scope) {
      object evaluationOutput;
      if (scopeToOutput.TryGetValue(scope, out evaluationOutput)) {
        return evaluationOutput;
      }
      return null;
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder("EvaluationContext(Configurations: [");
      ToString(sb, ScopeDefinitions.ConfigDefinitions.Keys);
      sb.Append("], Tasks: [");
      ToString(sb, ScopeDefinitions.TaskDefinitions.Keys);
      return sb.Append("])").ToString();
    }

    static void ToString(StringBuilder sb, IEnumerable<Scope> scopes) {
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