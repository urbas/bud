using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public class EnsureConfigInitialized<T> : Setting {
    public Func<EvaluationContext, T> InitialValue;

    public EnsureConfigInitialized(ConfigKey<T> key, T initialValue) : this(key, b => initialValue) {}

    public EnsureConfigInitialized(ConfigKey<T> key, Func<EvaluationContext, T> initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<Scope, IValueDefinition>.Builder buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = new ConfigDefinition<T>(InitialValue);
      }
    }
  }
}

