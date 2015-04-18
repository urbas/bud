using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud.SettingsConstruction {
  public class InitializeTask<T> : TaskModifier {
    private readonly Func<IContext, Task<T>> InitialValue;

    public InitializeTask(TaskKey key, T initialValue) : this(key, b => Task.FromResult(initialValue)) {}

    public InitializeTask(TaskKey key, Func<IContext, Task<T>> initialValue) : base(key) {
      InitialValue = initialValue;
    }

    public override void Modify(IDictionary<TaskKey, ITaskDefinition> buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = new TaskDefinition<T>(InitialValue);
      }
    }
  }

  public class InitializeTask : TaskModifier {
    private readonly Func<IContext, Task> TaskDefinition;

    public InitializeTask(TaskKey key, Func<IContext, Task> taskDefinition) : base(key) {
      TaskDefinition = taskDefinition;
    }

    public override void Modify(IDictionary<TaskKey, ITaskDefinition> buildConfigurationBuilder) {
      if (!buildConfigurationBuilder.ContainsKey(Key)) {
        buildConfigurationBuilder[Key] = new TaskDefinition(TaskDefinition);
      }
    }
  }
}