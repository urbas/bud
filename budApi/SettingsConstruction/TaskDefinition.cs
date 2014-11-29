using System;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace Bud.SettingsConstruction {

  public interface ITaskDefinition<out T> : IValueDefinition<T> {
    ImmutableHashSet<ITaskKey> Dependencies { get; }
    ITaskDefinition<T> WithDependencies(IEnumerable<ITaskKey> newDependencies);
  }

  public class TaskDefinition<T> : ITaskDefinition<T> {
    public readonly Func<BuildConfiguration, T> TaskFunction;

    public TaskDefinition(Func<T> taskFunction) : this(b => taskFunction()) {}

    public TaskDefinition(Func<BuildConfiguration, T> taskFunction) : this(taskFunction, ImmutableHashSet<ITaskKey>.Empty) {}

    public TaskDefinition(Func<BuildConfiguration, T> taskFunction, ImmutableHashSet<ITaskKey> dependencies) {
      TaskFunction = taskFunction;
      Dependencies = dependencies;
    }

    public ImmutableHashSet<ITaskKey> Dependencies { get; private set; }

    public ITaskDefinition<T> WithDependencies(IEnumerable<ITaskKey> newDependencies) {
      return new TaskDefinition<T>(TaskFunction, Dependencies.Union(newDependencies));
    }

    public T Evaluate(BuildConfiguration buildConfiguration) {
      return TaskFunction(buildConfiguration);
    }
  }

  public class TaskModification<T> : ITaskDefinition<T> {
    public readonly ITaskDefinition<T> ExistingValue;
    public readonly Func<BuildConfiguration, Func<T>, T> ValueModifier;

    public TaskModification(ITaskDefinition<T> existingValue, Func<BuildConfiguration, Func<T>, T> valueModifier) : this(existingValue, valueModifier, existingValue.Dependencies) {}

    public TaskModification(ITaskDefinition<T> existingValue, Func<BuildConfiguration, Func<T>, T> valueModifier, ImmutableHashSet<ITaskKey> dependencies) {
      ValueModifier = valueModifier;
      ExistingValue = existingValue;
      Dependencies = dependencies;
    }

    public ImmutableHashSet<ITaskKey> Dependencies { get; private set; }

    public ITaskDefinition<T> WithDependencies(IEnumerable<ITaskKey> newDependencies) {
      return new TaskModification<T>(ExistingValue, ValueModifier, Dependencies.Union(newDependencies));
    }

    public T Evaluate(BuildConfiguration buildConfiguration) {
      return ValueModifier(buildConfiguration, () => ExistingValue.Evaluate(buildConfiguration));
    }
  }

}

