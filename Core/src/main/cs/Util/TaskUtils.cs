using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bud.Util {
  public static class TaskUtils {
    public static readonly Func<IContext, Task> NoOpTask = context => NullAsyncResult;
    public static readonly Task<object> NullAsyncResult = Task.FromResult<object>(null);

    public static Task<T> ExecuteGuarded<T>(SemaphoreSlim semaphore, Func<Task<T>> actionToGuard) {
      return semaphore.WaitAsync()
                      .ContinueWith(t => actionToGuard())
                      .ContinueWith(t => {
                        semaphore.Release();
                        return t.Result;
                      })
                      .Unwrap();
    }

    public static Task ExecuteGuarded(SemaphoreSlim semaphore, Func<Task> actionToGuard) {
      return semaphore
        .WaitAsync()
        .ContinueWith(t => actionToGuard())
        .ContinueWith(t => {
          semaphore.Release();
          return t.Result;
        })
        .Unwrap();
    }
  }
}

