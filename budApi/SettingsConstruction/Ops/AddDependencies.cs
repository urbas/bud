using System;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace Bud.SettingsConstruction.Ops {
  public class AddDependencies<T> : Setting {
    private IEnumerable<ITaskKey> extraDependencies;

    public AddDependencies(TaskKey<T> key, IEnumerable<ITaskKey> extraDependencies) : base(key) {
      this.extraDependencies = extraDependencies;
    }

    public override void ApplyTo(ImmutableDictionary<ISettingKey, IValueDefinition>.Builder buildConfigurationBuilder) {
      IValueDefinition value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        ITaskDefinition<T> existingValue = (ITaskDefinition<T>)value;
        buildConfigurationBuilder[Key] = existingValue.WithDependencies(extraDependencies);
      } else {
        throw new InvalidOperationException(string.Format("Cannot modify the task '{0}'. This task has not yet been defined.", Key.GetType().FullName));
      }
    }
  }
}

