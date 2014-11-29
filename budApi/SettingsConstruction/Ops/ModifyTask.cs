using System;
using System.Collections.Immutable;

namespace Bud.SettingsConstruction.Ops {
  public static class ModifyTask {
    public static ModifyTask<T> Create<T>(TaskKey<T> key, Func<Func<T>, BuildConfiguration, T> taskModificationFunction) {
      return new ModifyTask<T>(key, taskModificationFunction);
    }
  }

  public class ModifyTask<T> : Setting {
    public Func<Func<T>, BuildConfiguration, T> TaskModification;

    public ModifyTask(TaskKey<T> key, Func<Func<T>, BuildConfiguration, T> taskModification) : base(key) {
      this.TaskModification = taskModification;
    }

    public override void ApplyTo(ImmutableDictionary<ISettingKey, object>.Builder buildConfigurationBuilder) {
      object value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        ITaskDefinition<T> existingValue = (ITaskDefinition<T>)value;
        buildConfigurationBuilder[Key] = new GenericTaskDefinition<T>(buildConfiguration => TaskModification(() => existingValue.Evaluate(buildConfiguration), buildConfiguration));
      } else {
        throw new InvalidOperationException(string.Format("Cannot modify the task '{0}'. This task has not yet been defined.", Key.GetType().FullName));
      }
    }
  }
}

