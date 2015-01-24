using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud {

  public interface ITaskDefinition {
    Task Evaluate(IContext context);
    ITaskDefinition WithDependencies(IEnumerable<TaskKey> newDependencies);
  }

  public class TaskDefinition<T> : ITaskDefinition {

    public readonly Func<IContext, Task<T>> TaskFunction;

    public TaskDefinition(Func<IContext, Task<T>> taskFunction) : this(taskFunction, ImmutableHashSet<TaskKey>.Empty) {}

    public TaskDefinition(Func<IContext, Task<T>> taskFunction, ImmutableHashSet<TaskKey> dependencies) {
      TaskFunction = taskFunction;
      Dependencies = dependencies;
    }

    public ImmutableHashSet<TaskKey> Dependencies { get; private set; }

    public ITaskDefinition WithDependencies(IEnumerable<TaskKey> newDependencies) {
      return new TaskDefinition<T>(TaskFunction, Dependencies.Union(newDependencies));
    }

    Task ITaskDefinition.Evaluate(IContext context) {
      return Evaluate(context);
    }

    public async Task<T> Evaluate(IContext context) {
      await InvokeDependencies(context);
      return await TaskFunction(context);
    }

    private async Task InvokeDependencies(IContext context) {
      foreach (var dependency in Dependencies) {
        await context.Evaluate(dependency);
      }
    }
  }

  public class TaskDefinition : ITaskDefinition {

    public readonly Func<IContext, Task> TaskFunction;

    public TaskDefinition(Func<IContext, Task> taskFunction) : this(taskFunction, ImmutableHashSet<TaskKey>.Empty) {}

    public TaskDefinition(Func<IContext, Task> taskFunction, ImmutableHashSet<TaskKey> dependencies) {
      TaskFunction = taskFunction;
      Dependencies = dependencies;
    }

    public ImmutableHashSet<TaskKey> Dependencies { get; private set; }

    public ITaskDefinition WithDependencies(IEnumerable<TaskKey> newDependencies) {
      return new TaskDefinition(TaskFunction, Dependencies.Union(newDependencies));
    }

    Task ITaskDefinition.Evaluate(IContext context) {
      return Evaluate(context);
    }

    public async Task Evaluate(IContext context) {
      await InvokeDependencies(context);
      await TaskFunction(context);
    }

    private async Task InvokeDependencies(IContext context) {
      foreach (var dependency in Dependencies) {
        await context.Evaluate(dependency);
      }
    }
  }

}