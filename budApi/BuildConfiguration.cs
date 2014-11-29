using System;
using System.Collections.Immutable;
using System.Text;
using Bud.SettingsConstruction;

namespace Bud {
  public class BuildConfiguration {
    public readonly ImmutableDictionary<ISettingKey, object> SettingKeysToValues;

    public BuildConfiguration(ImmutableDictionary<ISettingKey, object> settingKeysToValues) {
      this.SettingKeysToValues = settingKeysToValues;
    }

    public T Evaluate<T>(ConfigKey<T> key) {
      return (T)SettingKeysToValues[key];
    }

    public T Evaluate<T>(TaskKey<T> key) {
      return ((ITaskDefinition<T>)SettingKeysToValues[key]).Evaluate(this);
    }

    public object Evaluate(IValuedKey key) {
      return Evaluate((IValuedKey<object>)key);
    }

    public T Evaluate<T>(IValuedKey<T> key) {
      if (key is IConfigKey) {
        return (T)SettingKeysToValues[key];
      } else if (key is ITaskKey) {
        return ((ITaskDefinition<T>)SettingKeysToValues[key]).Evaluate(this);
      } else {
        throw new NotSupportedException("Could not evaluate a setting key of an unkown type.");
      }
    }

    public override string ToString() {
      if (SettingKeysToValues.IsEmpty)
        return "BuildConfiguration";
      StringBuilder sb = new StringBuilder("BuildConfiguration{");
      var enumerator = SettingKeysToValues.Keys.GetEnumerator();
      enumerator.MoveNext();
      AppendValueOf(sb, enumerator.Current);
      while (enumerator.MoveNext()) {
        var settingKey = enumerator.Current;
        sb.Append(", ");
        AppendValueOf(sb, settingKey);
      }
      return sb.Append('}').ToString();
    }

    private StringBuilder AppendValueOf(StringBuilder sb, ISettingKey settingKey) {
      sb.Append(settingKey);
      if (settingKey is IValuedKey) {
        sb.Append(" => ").Append(Evaluate((IValuedKey)settingKey));
      }
      return sb;
    }
  }

}