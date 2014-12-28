using System;

namespace Bud.SettingsConstruction {
  public class ConfigDefinition<T> : IValueDefinition {
    public readonly Func<EvaluationContext, T> ConfigValue;

    public ConfigDefinition(Func<EvaluationContext, T> configValue) {
      this.ConfigValue = configValue;
    }

    object IValueDefinition.Evaluate(EvaluationContext context) {
      return Evaluate(context);
    }

    public T Evaluate(EvaluationContext context) {
      return ConfigValue(context);
    }
  }
}

