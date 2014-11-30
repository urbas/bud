using System;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace Bud.SettingsConstruction {
  public class TaskDefinition<T> : IValueDefinition<T> {
    public readonly Func<BuildConfiguration, T> TaskFunction;

    public TaskDefinition(Func<T> taskFunction) : this(b => taskFunction()) {}

    public TaskDefinition(Func<BuildConfiguration, T> taskFunction) : this(taskFunction, ImmutableHashSet<ITaskKey>.Empty) {}

    public TaskDefinition(Func<BuildConfiguration, T> taskFunction, ImmutableHashSet<ITaskKey> dependencies) {
      TaskFunction = taskFunction;
      Dependencies = dependencies;
    }

    public ImmutableHashSet<ITaskKey> Dependencies { get; private set; }

    public TaskDefinition<T> WithDependencies(IEnumerable<ITaskKey> newDependencies) {
      return new TaskDefinition<T>(TaskFunction, Dependencies.Union(newDependencies));
    }

    public T Evaluate(BuildConfiguration buildConfiguration) {
      InvokeDependencies(buildConfiguration);
      return TaskFunction(buildConfiguration);
    }

    private void InvokeDependencies(BuildConfiguration buildConfiguration) {
      foreach (var dependency in Dependencies) {
        buildConfiguration.Evaluate(dependency);
      }
    }
  }

}

