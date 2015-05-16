using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Bud.Util;

namespace Bud.SettingsConstruction {
  public class TaskDefinition : TaskDependencies, ITaskDefinition {
    public readonly Func<IContext, Task> TaskFunction;

    public TaskDefinition(Func<IContext, Task> taskFunction) : this(taskFunction, ImmutableHashSet<TaskKey>.Empty) {}

    public TaskDefinition(Func<IContext, Task> taskFunction, ImmutableHashSet<TaskKey> dependencies) : base(dependencies) {
      TaskFunction = taskFunction;
    }

    public ITaskDefinition WithDependencies(IEnumerable<TaskKey> newDependencies) {
      return new TaskDefinition(TaskFunction, Dependencies.Union(newDependencies));
    }

    public Task EvaluateGuarded(IContext context, Key key, SemaphoreSlim semaphore, Func<Task> cachedValueGetter, Action<Task> valueCacher) {
      return TaskUtils.ExecuteGuarded(semaphore, () => {
        Task cachedValue = cachedValueGetter();
        if (cachedValue == null) {
          Task evaluationTask = Evaluate(context, key);
          valueCacher(evaluationTask);
          return evaluationTask;
        }
        return cachedValue;
      });
    }

    public object ExtractResult(Task completedTask) => null;

    Task ITaskDefinition.Evaluate(IContext context, Key taskKey) => Evaluate(context, taskKey);

    public async Task Evaluate(IContext context, Key taskKey) {
      await InvokeDependencies(context);
      try {
        await TaskFunction(context);
      } catch (Exception e) {
        throw new Exception(EvaluationErrorMessage(taskKey), e);
      }
    }

    public static string EvaluationErrorMessage(Key taskKey) => $"Task '{taskKey}' failed.";
  }

  public class TaskDefinition<T> : TaskDependencies, ITaskDefinition {
    public readonly Func<IContext, Task<T>> TaskFunction;

    public TaskDefinition(Func<IContext, Task<T>> taskFunction) : this(taskFunction, ImmutableHashSet<TaskKey>.Empty) {}

    public TaskDefinition(Func<IContext, Task<T>> taskFunction, ImmutableHashSet<TaskKey> dependencies) : base(dependencies) {
      TaskFunction = taskFunction;
    }

    public async Task<T> Evaluate(IContext context, Key taskKey) {
      await InvokeDependencies(context);
      try {
        return await TaskFunction(context);
      } catch (Exception e) {
        throw new Exception(TaskDefinition.EvaluationErrorMessage(taskKey), e);
      }
    }

    public ITaskDefinition WithDependencies(IEnumerable<TaskKey> newDependencies) => new TaskDefinition<T>(TaskFunction, Dependencies.Union(newDependencies));

    public Task EvaluateGuarded(IContext context, Key key, SemaphoreSlim semaphore, Func<Task> cachedValueGetter, Action<Task> valueCacher) {
      return TaskUtils.ExecuteGuarded(semaphore, () => {
        Task<T> cachedValue = (Task<T>) cachedValueGetter();
        if (cachedValue == null) {
          Task<T> evaluationTask = Evaluate(context, key);
          valueCacher(evaluationTask);
          return evaluationTask;
        }
        return cachedValue;
      });
    }

    public object ExtractResult(Task completedTask) => ((Task<T>) completedTask).Result;

    Task ITaskDefinition.Evaluate(IContext context, Key taskKey) => Evaluate(context, taskKey);
  }
}