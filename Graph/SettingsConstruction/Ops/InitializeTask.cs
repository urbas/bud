using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.SettingsConstruction.Ops {
  public class InitializeTask<T> : TaskDefinitionConstructor {
    public Func<EvaluationContext, Task<T>> InitialValue;

    public InitializeTask(TaskKey<T> key, T initialValue) : this(key, b => Task.FromResult(initialValue)) {}

    public InitializeTask(TaskKey<T> key, Func<EvaluationContext, Task<T>> initialValue) : base(key) {
      this.InitialValue = initialValue;
    }

    public override void ApplyTo(ImmutableDictionary<Scope, ITaskDefinition>.Builder buildConfigurationBuilder) {
      if (buildConfigurationBuilder.ContainsKey(Key)) {
        throw new InvalidOperationException(string.Format("Cannot initialize the task '{0}'. It has already been initialized.", Key));
      }
      buildConfigurationBuilder[Key] = new TaskDefinition<T>(InitialValue);
    }
  }
}

