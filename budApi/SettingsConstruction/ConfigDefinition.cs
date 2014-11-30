using System;

namespace Bud.SettingsConstruction {
  public class ConfigDefinition<T> : IValueDefinition<T> {
    public readonly Func<BuildConfiguration, T> ConfigValue;

    public ConfigDefinition(Func<BuildConfiguration, T> configValue) {
      this.ConfigValue = configValue;
    }

    public T Evaluate(BuildConfiguration buildConfiguration) {
      return ConfigValue(buildConfiguration);
    }
  }
}

