using System;

namespace Bud {

  public interface IConfigDefinition {
    object Evaluate(Configuration context);
  }

  public class ConfigDefinition<T> : IConfigDefinition {
    public readonly Func<Configuration, T> ConfigValue;

    public ConfigDefinition(Func<Configuration, T> configValue) {
      this.ConfigValue = configValue;
    }

    object IConfigDefinition.Evaluate(Configuration context) {
      return Evaluate(context);
    }

    public T Evaluate(Configuration context) {
      return ConfigValue(context);
    }
  }
}

