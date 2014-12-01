using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud.SettingsConstruction {
  public interface ITaskDefinition : IValueDefinition {
    Task Evaluate(EvaluationContext buildConfiguration);
  }

  public class TaskDefinition<T> : ITaskDefinition {
    public readonly Func<EvaluationContext, Task<T>> TaskFunction;

    public TaskDefinition(Func<EvaluationContext, Task<T>> taskFunction) : this(taskFunction, ImmutableHashSet<ITaskKey>.Empty) {}

    public TaskDefinition(Func<EvaluationContext, Task<T>> taskFunction, ImmutableHashSet<ITaskKey> dependencies) {
      TaskFunction = taskFunction;
      Dependencies = dependencies;
    }

    public ImmutableHashSet<ITaskKey> Dependencies { get; private set; }

    public TaskDefinition<T> WithDependencies(IEnumerable<ITaskKey> newDependencies) {
      return new TaskDefinition<T>(TaskFunction, Dependencies.Union(newDependencies));
    }

    Task ITaskDefinition.Evaluate(EvaluationContext context) {
      return Evaluate(context);
    }

    public async Task<T> Evaluate(EvaluationContext context) {
      await InvokeDependencies(context);
      return await TaskFunction(context);
    }

    async private Task InvokeDependencies(EvaluationContext context) {
      foreach (var dependency in Dependencies) {
        await context.Evaluate(dependency);
      }
    }
  }

}

