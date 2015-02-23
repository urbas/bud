using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.SettingsConstruction {
  public class ModifyTask : TaskDefinitionConstructor {
    public Func<IContext, Func<Task>, Task> TaskModification;

    public ModifyTask(TaskKey key, Func<IContext, Func<Task>, Task> taskModification) : base(key) {
      TaskModification = taskModification;
    }

    public override void ApplyTo(ImmutableDictionary<Key, ITaskDefinition>.Builder buildConfigurationBuilder) {
      ITaskDefinition taskDefinition;
      if (buildConfigurationBuilder.TryGetValue(Key, out taskDefinition)) {
        buildConfigurationBuilder[Key] = new TaskDefinition(context => TaskModification(context, () => context.Evaluate(taskDefinition, Key)));
      } else {
        throw new InvalidOperationException(string.Format("Cannot modify the task '{0}'. This task has not yet been defined.", Key));
      }
    }
  }

  public class ModifyTask<T> : TaskDefinitionConstructor {
    public Func<IContext, Func<Task<T>>, Task<T>> TaskModification;

    public ModifyTask(TaskKey<T> key, Func<IContext, Func<Task<T>>, Task<T>> taskModification) : base(key) {
      TaskModification = taskModification;
    }

    public override void ApplyTo(ImmutableDictionary<Key, ITaskDefinition>.Builder buildConfigurationBuilder) {
      ITaskDefinition value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        TaskDefinition<T> existingTaskDef = (TaskDefinition<T>)value;
        buildConfigurationBuilder[Key] = new TaskDefinition<T>(context => TaskModification(context, () => context.Evaluate(existingTaskDef, Key)));
      } else {
        throw new InvalidOperationException(string.Format("Cannot modify the task '{0}'. This task has not yet been defined.", Key));
      }
    }
  }
}

