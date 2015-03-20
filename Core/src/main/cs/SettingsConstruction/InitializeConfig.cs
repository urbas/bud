using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction {
  public class InitializeConfig<T> : ConfigModifier {
    public Func<IConfig, T> InitialValue;

    public InitializeConfig(ConfigKey<T> key, T initialValue) : this(key, b => initialValue) {}

    public InitializeConfig(ConfigKey<T> key, Func<IConfig, T> initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<Key, IConfigDefinition>.Builder buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = new ConfigDefinition<T>(InitialValue);
      }
    }
  }
}

