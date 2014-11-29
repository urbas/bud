using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public class InitializeConfig<T> : Setting {
    public Func<BuildConfiguration, T> InitialValue;

    public InitializeConfig(SettingKey key, T initialValue) : this(key, b => initialValue) {}

    public InitializeConfig(SettingKey key, Func<BuildConfiguration, T> initialValue) : base(key) {
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

