using System;
using System.Collections.Immutable;
using System.Text;
using Bud.SettingsConstruction;

namespace Bud {
  public class BuildConfiguration {
    public readonly ImmutableDictionary<ISettingKey, IValueDefinition> SettingKeysToValues;

    private BuildConfiguration(ImmutableDictionary<ISettingKey, IValueDefinition> settingKeysToValues) {
      this.SettingKeysToValues = settingKeysToValues;
    }

    public T Evaluate<T>(IValuedKey<T> key) {
      return ((IValueDefinition<T>)SettingKeysToValues[key]).Evaluate(this);
    }

    public object Evaluate(IValuedKey key) {
      return Evaluate((IValuedKey<object>)key);
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

    public static BuildConfiguration ToBuildConfiguration(Settings settings) {
      var buildConfigurationBuilder = ImmutableDictionary.CreateBuilder<ISettingKey, IValueDefinition>();
      foreach (var setting in settings.SettingsList) {
        setting.ApplyTo(buildConfigurationBuilder);
      }
      return new BuildConfiguration(buildConfigurationBuilder.ToImmutable());
    }
  }

}