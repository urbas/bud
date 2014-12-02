using System;

namespace Bud.SettingsConstruction {
  public class ConfigDefinition<T> : IValueDefinition {
    public readonly Func<EvaluationContext, T> ConfigValue;

    public ConfigDefinition(Func<EvaluationContext, T> configValue) {
      this.ConfigValue = configValue;
    }

    public T Evaluate(EvaluationContext buildConfiguration) {
      return ConfigValue(buildConfiguration);
    }
  }
}

