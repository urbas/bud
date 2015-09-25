using System;
using System.Threading.Tasks;

namespace Bud.Tasking {
  public interface ITaskDefinition {
    Type ReturnType { get; }
    Func<ITasker, Task> Task { get; }
  }
}