using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public class EnsureConfigInitialized<T> : ConfigDefinitionConstructor {
    public Func<Configuration, T> InitialValue;

    public EnsureConfigInitialized(ConfigKey<T> key, T initialValue) : this(key, b => initialValue) {}

    public EnsureConfigInitialized(ConfigKey<T> key, Func<Configuration, T> initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<Scope, IConfigDefinition>.Builder buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = new ConfigDefinition<T>(InitialValue);
      }
    }
  }
}

