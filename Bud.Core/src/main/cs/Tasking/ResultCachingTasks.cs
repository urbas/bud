using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.Tasking {
  internal class ResultCachingTasks : ITasks {
    private IDictionary<string, TaskDefinition> Tasks { get; }
    private ImmutableDictionary<string, TaskResult> taskResultCache = ImmutableDictionary<string, TaskResult>.Empty;
    private readonly object taskResultCacheGuard = new object();

    internal ResultCachingTasks(IDictionary<string, TaskDefinition> tasks) {
      Tasks = tasks;
    }

    public Task<T> Invoke<T>(string taskName) {
      var taskResult = GetFromCacheOrInvoke(taskName);
      AssertTaskTypedCorrectly<T>(taskName, taskResult.ResultType);
      return (Task<T>) taskResult.Result;
    }

    public Task Invoke(string taskName) => GetFromCacheOrInvoke(taskName).Result;

    private TaskResult GetFromCacheOrInvoke(string taskName) {
      TaskResult taskResult;
      return taskResultCache.TryGetValue(taskName, out taskResult) ? taskResult : CalculateTaskResult(taskName);
    }

    private TaskResult CalculateTaskResult(string taskName) {
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

    private static void AssertTaskTypedCorrectly<T>(string taskName, Type actualTaskReturnType) {
      if (actualTaskReturnType != typeof(T)) {
        throw new TaskReturnsDifferentTypeException($"Task '{taskName}' returns '{actualTaskReturnType.FullName}' but was expected to return '{typeof(T).FullName}'.");
      }
    }

    private struct TaskResult {
      public Type ResultType;
      public Task Result;
    }
  }
}