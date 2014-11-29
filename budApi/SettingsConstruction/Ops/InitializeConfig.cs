using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public class InitializeConfig<T> : Setting {
    public Func<BuildConfiguration, T> InitialValue;

    public InitializeConfig(ConfigKey<T> key, T initialValue) : this(key, b => initialValue) {}

    public InitializeConfig(ConfigKey<T> key, Func<BuildConfiguration, T> initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<ISettingKey, IValueDefinition>.Builder buildConfigurationBuilder) {
      if (buildConfigurationBuilder.ContainsKey(Key)) {
        throw new InvalidOperationException(string.Format("The setting '{0}' has already been initialized.", Key));
      }
      buildConfigurationBuilder[Key] = new ConfigDefinition<T>(InitialValue);
    }
  }
}

