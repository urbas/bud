using System;

namespace Bud {

  public interface IConfigDefinition {
    object Evaluate(IConfig context);
  }

  public class ConfigDefinition<T> : IConfigDefinition {
    public readonly Func<IConfig, T> ConfigValue;

    public ConfigDefinition(Func<IConfig, T> configValue) {
      this.ConfigValue = configValue;
    }

    object IConfigDefinition.Evaluate(IConfig context) {
      return Evaluate(context);
    }

    public T Evaluate(IConfig context) {
      return ConfigValue(context);
    }
  }
}

