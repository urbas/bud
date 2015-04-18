using System;
using System.Collections.Generic;

namespace Bud.SettingsConstruction {
  public class InitializeConfig<T> : ConfigModifier {
    public Func<IConfig, T> InitialValue;

    public InitializeConfig(ConfigKey<T> key, T initialValue) : this(key, b => initialValue) {}

    public InitializeConfig(ConfigKey<T> key, Func<IConfig, T> initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void Modify(IDictionary<ConfigKey, IConfigDefinition> buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = new ConfigDefinition<T>(InitialValue);
      }
    }
  }
}