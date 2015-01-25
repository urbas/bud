using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud {
  public interface ITaskDefinition {
    ImmutableHashSet<TaskKey> Dependencies { get; }
    Task Evaluate(IContext context);
    ITaskDefinition WithDependencies(IEnumerable<TaskKey> newDependencies);
  }

  public class TaskDefinition<T> : TaskDependencies, ITaskDefinition {
    public readonly Func<IContext, Task<T>> TaskFunction;

    public TaskDefinition(Func<IContext, Task<T>> taskFunction) : this(taskFunction, ImmutableHashSet<TaskKey>.Empty) {}

    public TaskDefinition(Func<IContext, Task<T>> taskFunction, ImmutableHashSet<TaskKey> dependencies) : base(dependencies) {
      TaskFunction = taskFunction;
    }

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
  }

  public class TaskDefinition : TaskDependencies, ITaskDefinition {
    public readonly Func<IContext, Task> TaskFunction;

    public TaskDefinition(Func<IContext, Task> taskFunction) : this(taskFunction, ImmutableHashSet<TaskKey>.Empty) {}

    public TaskDefinition(Func<IContext, Task> taskFunction, ImmutableHashSet<TaskKey> dependencies) : base(dependencies) {
      TaskFunction = taskFunction;
    }

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
  }
}