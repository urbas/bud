using System;
using System.Threading.Tasks;

namespace Bud.Tasking.ApiV1 {
  public class TaskDefinition {
    public Type ReturnType { get; }
    public Func<ITasks, Task> Task { get; }

    public TaskDefinition(Type returnType, Func<ITasks, Task> task) {
      ReturnType = returnType;
      Task = task;
    }
  }
}