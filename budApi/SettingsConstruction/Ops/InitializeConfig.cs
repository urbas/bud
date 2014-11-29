using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public static class InitializeConfig {
    public static InitializeConfig<T> Create<T>(SettingKey key, T initialValue) {
      return new InitializeConfig<T>(key, initialValue);
    }
  }

  public class InitializeConfig<T> : Setting {
    public T InitialValue;

    public InitializeConfig(SettingKey key, T initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<ISettingKey, object>.Builder buildConfigurationBuilder) {
      if (buildConfigurationBuilder.ContainsKey(Key)) {
        throw new InvalidOperationException(string.Format("The setting '{0}' has already been initialized.", Key));
      }
      buildConfigurationBuilder[Key] = InitialValue;
    }
  }
}

