using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public class InitializeTask<T> : Setting {
    public Func<BuildConfiguration, T> InitialValue;

    public InitializeTask(TaskKey<T> key, T initialValue) : this(key, b => initialValue) {}

    public InitializeTask(TaskKey<T> key, Func<BuildConfiguration, T> initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<ISettingKey, IValueDefinition>.Builder buildConfigurationBuilder) {
      if (buildConfigurationBuilder.ContainsKey(Key)) {
        throw new InvalidOperationException(string.Format("Cannot initialize the task '{0}'. It has already been initialized.", Key));
      }
      buildConfigurationBuilder[Key] = new TaskDefinition<T>(InitialValue);
    }
  }
}

