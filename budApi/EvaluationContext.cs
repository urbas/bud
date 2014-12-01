using System;
using System.Collections.Immutable;
using System.Text;
using Bud.SettingsConstruction;
using Bud.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud {
  public class EvaluationContext {
    public readonly IDictionary<ISettingKey, IValueDefinition> SettingKeysToValues;
    // TODO: Assign sequential indices to keys and use arrays to access evaluated values (instead of dictionaries).
    private readonly Dictionary<ConfigKey, object> configValues = new Dictionary<ConfigKey, object>();
    private readonly Dictionary<TaskKey, Task> taskValues = new Dictionary<TaskKey, Task>();

    private EvaluationContext(IDictionary<ISettingKey, IValueDefinition> settingKeysToValues) {
      this.SettingKeysToValues = settingKeysToValues;
    }

    public T Evaluate<T>(ConfigKey<T> key) {
      object value;
      if (configValues.TryGetValue(key, out value)) {
        return (T)value;
      } else {
        var evaluatedValue = ((ConfigDefinition<T>)SettingKeysToValues[key]).Evaluate(this);
        configValues.Add(key, evaluatedValue);
        return evaluatedValue;
      }
    }

    public Task Evaluate(TaskKey key) {
      Task value;
      if (taskValues.TryGetValue(key, out value)) {
        return value;
      } else {
        var evaluatedValue = ((ITaskDefinition)SettingKeysToValues[key]).Evaluate(this);
        taskValues.Add(key, evaluatedValue);
        return evaluatedValue;
      }
    }

    public Task<T> Evaluate<T>(TaskKey<T> key) {
      return (Task<T>)Evaluate((TaskKey)key);
    }

    public static EvaluationContext ToEvaluationContext(Settings settings) {
      return new EvaluationContext(SettingsUtils.ToCompiledSettings(settings));
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder("EvaluationContext{");
      var enumerator = SettingKeysToValues.Keys.GetEnumerator();
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