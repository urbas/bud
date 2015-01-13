using System;
using System.Threading.Tasks;

namespace Bud.Util {
  public static class TaskUtils {
    public static readonly Task<Unit> UnitTask = Task.FromResult(Unit.Instance);
    public static readonly Func<IContext, Task<Unit>> NoOpTask = context => UnitTask;
  }
}

