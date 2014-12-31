using System;

namespace Bud.SettingsConstruction {
  public interface IConfigDefinition {
    object Evaluate(IConfiguration context);
  }

  public class ConfigDefinition<T> : IConfigDefinition {
    public readonly Func<EvaluationContext, T> ConfigValue;

    public ConfigDefinition(Func<EvaluationContext, T> configValue) {
      this.ConfigValue = configValue;
    }

    object IConfigDefinition.Evaluate(IConfiguration context) {
      return Evaluate(context);
    }

    public T Evaluate(IConfiguration context) {
      return ConfigValue(context);
    }
  }
}

