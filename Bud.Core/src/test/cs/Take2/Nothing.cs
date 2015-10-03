using System;
using System.Threading.Tasks;
using Bud.Tasking;

namespace Bud.Take2 {
  public sealed class Nothing {
    public static readonly Nothing Instance = new Nothing();
    public static readonly Task<Nothing> TaskResult = Task.FromResult(Instance);
    public static readonly Func<ITasks, Task<Nothing>> NoOp = tasker => TaskResult;

    private Nothing() {}
  }
}