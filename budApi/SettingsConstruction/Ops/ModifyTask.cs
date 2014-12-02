using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.SettingsConstruction.Ops {
  public class ModifyTask<T> : Setting {
    public Func<EvaluationContext, Func<Task<T>>, Task<T>> TaskModification;

    public ModifyTask(TaskKey<T> key, Func<EvaluationContext, Func<Task<T>>, Task<T>> taskModification) : base(key) {
      this.TaskModification = taskModification;
    }

    public override void ApplyTo(ImmutableDictionary<Scope, IValueDefinition>.Builder buildConfigurationBuilder) {
      IValueDefinition value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        TaskDefinition<T> existingTaskDef = (TaskDefinition<T>)value;
        buildConfigurationBuilder[Key] = new TaskDefinition<T>(context => TaskModification(context, () => context.Evaluate(existingTaskDef)));
      } else {
        throw new InvalidOperationException(string.Format("Cannot modify the task '{0}'. This task has not yet been defined.", Key.GetType().FullName));
      }
    }
  }
}

