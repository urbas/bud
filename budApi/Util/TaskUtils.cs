using System;
using System.Threading.Tasks;

namespace Bud.Util {
  public static class TaskUtils {
    public static readonly Func<EvaluationContext, Task<Unit>> NoOpTask = async context => Unit.Instance;
  }
}

