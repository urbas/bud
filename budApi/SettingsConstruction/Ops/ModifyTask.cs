﻿using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public class ModifyTask<T> : Setting {
    public Func<BuildConfiguration, Func<T>, T> TaskModification;

    public ModifyTask(TaskKey<T> key, Func<BuildConfiguration, Func<T>, T> taskModification) : base(key) {
      this.TaskModification = taskModification;
    }

    public override void ApplyTo(ImmutableDictionary<ISettingKey, IValueDefinition>.Builder buildConfigurationBuilder) {
      IValueDefinition value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        ITaskDefinition<T> existingValue = (ITaskDefinition<T>)value;
        buildConfigurationBuilder[Key] = new TaskModification<T>(existingValue, TaskModification);
      } else {
        throw new InvalidOperationException(string.Format("Cannot modify the task '{0}'. This task has not yet been defined.", Key.GetType().FullName));
      }
    }
  }
}
