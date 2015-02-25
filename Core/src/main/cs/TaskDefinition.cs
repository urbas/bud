using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Util;

namespace Bud {
  public interface ITaskDefinition {
    ImmutableHashSet<TaskKey> Dependencies { get; }
    Task Evaluate(IContext context, Key taskKey);
    ITaskDefinition WithDependencies(IEnumerable<TaskKey> newDependencies);
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

    Task ITaskDefinition.Evaluate(IContext context, Key taskKey) {
      return Evaluate(context, taskKey);
    }

    public async Task Evaluate(IContext context, Key taskKey) {
      await InvokeDependencies(context);
      try {
        await TaskFunction(context);
      } catch (Exception e) {
        throw new Exception(EvaluationErrorMessage(taskKey), e);
      }
    }

    public static string EvaluationErrorMessage(Key taskKey) {
      return string.Format("Task '{0}' failed.", taskKey);
    }
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

    Task ITaskDefinition.Evaluate(IContext context, Key taskKey) {
      return Evaluate(context, taskKey);
    }

    public async Task<T> Evaluate(IContext context, Key taskKey) {
      await InvokeDependencies(context);
      try {
        return await TaskFunction(context);
      } catch (Exception e) {
        throw new Exception(TaskDefinition.EvaluationErrorMessage(taskKey), e);
      }
    }
  }
}