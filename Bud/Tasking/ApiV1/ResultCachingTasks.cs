using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.Tasking.ApiV1 {
  public class ResultCachingTasks : ITasks {
    private IDictionary<string, TaskDefinition> Tasks { get; }
    private ImmutableDictionary<string, TaskResult> taskResultCache = ImmutableDictionary<string, TaskResult>.Empty;
    private readonly object taskResultCacheGuard = new object();

    internal ResultCachingTasks(IDictionary<string, TaskDefinition> tasks) {
      Tasks = tasks;
    }

    public Task<T> Get<T>(Key<T> taskName) {
      var taskResult = GetFromCacheOrInvoke(taskName);
      TasksUtils.AssertTaskTypeIsSame<T>(taskName, taskResult.ResultType);
      return (Task<T>) taskResult.Result;
    }

    public Task Get(Key taskName) => GetFromCacheOrInvoke(taskName).Result;

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
        TaskDefinition taskDefinition;
        if (Tasks.TryGetValue(taskName, out taskDefinition)) {
          taskResult = new TaskResult {ResultType = taskDefinition.ReturnType, Result = taskDefinition.Task(this)};
          taskResultCache = taskResultCache.Add(taskName, taskResult);
          return taskResult;
        }
        throw new TaskUndefinedException($"Task '{taskName ?? "<null>"}' is undefined.");
      }
    }

    private struct TaskResult {
      public Type ResultType;
      public Task Result;
    }
  }
}