using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public class EnsureTaskInitialized<T> : Setting {
    public TaskDefinition<T> InitialValue;

    public EnsureTaskInitialized(TaskKey<T> key, TaskDefinition<T> initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<ISettingKey, IValueDefinition>.Builder buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = InitialValue;
      }
    }
  }
}

