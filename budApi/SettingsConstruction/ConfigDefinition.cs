using System;

namespace Bud.SettingsConstruction {
  public interface IConfigDefinition<out T> : IValueDefinition<T> {}

  public class ConfigDefinition<T> : IConfigDefinition<T> {
    public readonly Func<BuildConfiguration, T> ConfigValue;

    public ConfigDefinition(Func<BuildConfiguration, T> configValue) {
      this.ConfigValue = configValue;
    }

    public T Evaluate(BuildConfiguration buildConfiguration) {
      return ConfigValue(buildConfiguration);
    }
  }

  public class ConfigModification<T> : IConfigDefinition<T> {
    public readonly IValueDefinition<T> ExistingValue;
    public readonly Func<BuildConfiguration, T, T> ValueModifier;

    public ConfigModification(IValueDefinition<T> existingValue, Func<BuildConfiguration, T, T> valueModifier) {
      this.ValueModifier = valueModifier;
      this.ExistingValue = existingValue;
    }

    public T Evaluate(BuildConfiguration buildConfiguration) {
      return ValueModifier(buildConfiguration, ExistingValue.Evaluate(buildConfiguration));
    }
  }
}

