using System;
using System.Threading.Tasks;

namespace Bud.Util {
  public static class TaskUtils {
    public static readonly Func<IContext, Task> NoOpTask = context => NullAsyncResult;
    public static readonly Task<object> NullAsyncResult = Task.FromResult<object>(null);
  }
}

