using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud.SettingsConstruction {
  public class ModifyTask : TaskModifier {
    private readonly Func<IContext, Func<Task>, Task> TaskModification;

    public ModifyTask(TaskKey key, Func<IContext, Func<Task>, Task> taskModification) : base(key) {
      TaskModification = taskModification;
    }

    public override void Modify(IDictionary<TaskKey, ITaskDefinition> buildConfigurationBuilder) {
      ITaskDefinition taskDefinition;
      if (buildConfigurationBuilder.TryGetValue(Key, out taskDefinition)) {
        buildConfigurationBuilder[Key] = new TaskDefinition(context => TaskModification(context, () => context.Evaluate(taskDefinition, Key)));
      } else {
        throw new InvalidOperationException(TaskUndefinedErrorMessage(Key));
      }
    }

    internal static string TaskUndefinedErrorMessage(TaskKey taskKey) {
      return $"Cannot modify the task '{taskKey}'. This task has not yet been defined.";
    }
  }

  public class ModifyTask<T> : TaskModifier {
    private readonly Func<IContext, Func<Task<T>>, Task<T>> TaskModification;

    public ModifyTask(TaskKey key, Func<IContext, Func<Task<T>>, Task<T>> taskModification) : base(key) {
      TaskModification = taskModification;
    }

    public override void Modify(IDictionary<TaskKey, ITaskDefinition> buildConfigurationBuilder) {
      ITaskDefinition value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        var existingTaskDef = (TaskDefinition<T>) value;
        buildConfigurationBuilder[Key] = new TaskDefinition<T>(context => TaskModification(context, () => context.Evaluate(existingTaskDef, Key)));
      } else {
        throw new InvalidOperationException(ModifyTask.TaskUndefinedErrorMessage(Key));
      }
    }
  }
}