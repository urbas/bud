using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {

  public static class EnsureTaskInitialized {
    public static EnsureTaskInitialized<T> Create<T>(TaskKey<T> key, ITaskDefinition<T> initialValue) {
      return new EnsureTaskInitialized<T>(key, initialValue);
    }
  }

  public class EnsureTaskInitialized<T> : Setting {
    public ITaskDefinition<T> InitialValue;

    public EnsureTaskInitialized(TaskKey<T> key, ITaskDefinition<T> initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<ISettingKey, object>.Builder buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = InitialValue;
      }
    }
  }
}

