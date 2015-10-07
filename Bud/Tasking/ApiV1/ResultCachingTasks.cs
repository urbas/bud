using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Tasking.ApiV1 {
  public class ResultCachingTasks : ITasks {
    private IDictionary<string, ITaskDefinition> Tasks { get; }
    private ImmutableDictionary<string, TaskResult> taskResultCache = ImmutableDictionary<string, TaskResult>.Empty;
    private readonly object taskResultCacheGuard = new object();

    internal ResultCachingTasks(IDictionary<string, ITaskDefinition> tasks) {
      Tasks = tasks;
    }

    public T Get<T>(Key<T> taskName) {
      var taskResult = GetFromCacheOrInvoke(taskName);
      TasksUtils.AssertTaskTypeIsSame<T>(taskName, taskResult.ResultType);
      return (T) taskResult.Result;
    }

    private TaskResult GetFromCacheOrInvoke(string taskName) {
      TaskResult taskResult;
      return taskResultCache.TryGetValue(taskName, out taskResult) ? taskResult : InvokeTaskAndCache(taskName);
    }

    private TaskResult InvokeTaskAndCache(string taskName) {
      lock (taskResultCacheGuard) {
        TaskResult taskResult;
        if (taskResultCache.TryGetValue(taskName, out taskResult)) {
          return taskResult;
        }
        ITaskDefinition taskDefinition;
        if (Tasks.TryGetValue(taskName, out taskDefinition)) {
          taskResult = new TaskResult {ResultType = taskDefinition.ReturnType, Result = taskDefinition.Invoke(this)};
          taskResultCache = taskResultCache.Add(taskName, taskResult);
          return taskResult;
        }
        throw new TaskUndefinedException($"Task '{taskName ?? "<null>"}' is undefined.");
      }
    }

    private struct TaskResult {
      public Type ResultType;
      public object Result;
    }
  }
}