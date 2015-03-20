using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.SettingsConstruction {
  public class InitializeTask<T> : TaskModifier {
    public Func<IContext, Task<T>> InitialValue;

    public InitializeTask(TaskKey<T> key, T initialValue) : this(key, b => Task.FromResult(initialValue)) {}

    public InitializeTask(TaskKey<T> key, Func<IContext, Task<T>> initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<Key, ITaskDefinition>.Builder buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = new TaskDefinition<T>(InitialValue);
      }
    }
  }

  public class InitializeTask : TaskModifier {
    public Func<IContext, Task> TaskDefinition;

    public InitializeTask(TaskKey key, Func<IContext, Task> taskDefinition) : base(key) {
      TaskDefinition = taskDefinition;
    }

    public override void ApplyTo(ImmutableDictionary<Key, ITaskDefinition>.Builder buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = new TaskDefinition(TaskDefinition);
      }
    }
  }
}

