using System;
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
        TaskDefinition<T> existingValue = (TaskDefinition<T>)value;
        buildConfigurationBuilder[Key] = new TaskDefinition<T>(buildConfig => TaskModification(buildConfig, () => existingValue.Evaluate(buildConfig)));
      } else {
        throw new InvalidOperationException(string.Format("Cannot modify the task '{0}'. This task has not yet been defined.", Key.GetType().FullName));
      }
    }
  }
}

