using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.Tasking {
  internal class Tasker : ITasker {
    private Tasks Tasks { get; }
    private ImmutableDictionary<string, TaskResult> taskResultCache = ImmutableDictionary<string, TaskResult>.Empty;
    private readonly object taskResultCacheGuard = new object();

    internal Tasker(Tasks tasks) {
      Tasks = tasks;
    }

    public Task<T> Invoke<T>(string taskName) {
      Task<T> taskResult;
      if (TryGetCachedTaskResult(taskName, out taskResult)) {
        return taskResult;
      }
      lock (taskResultCacheGuard) {
        if (TryGetCachedTaskResult(taskName, out taskResult)) {
          return taskResult;
        }
        taskResult = Tasks.InvokeTask<T>(taskName, this);
        taskResultCache = taskResultCache.Add(taskName, new TaskResult {ResultType = typeof(T), Result = taskResult});
        return taskResult;
      }
    }

    private bool TryGetCachedTaskResult<T>(string taskName, out Task<T> taskResult) {
      TaskResult cachedTaskResult;
      if (taskResultCache.TryGetValue(taskName, out cachedTaskResult)) {
        AssertTaskTypedCorrectly<T>(taskName, cachedTaskResult.ResultType);
        taskResult = (Task<T>) cachedTaskResult.Result;
        return true;
      }
      taskResult = null;
      return false;
    }

    internal static void AssertTaskTypedCorrectly<T>(string taskName, Type actualTaskReturnType) {
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