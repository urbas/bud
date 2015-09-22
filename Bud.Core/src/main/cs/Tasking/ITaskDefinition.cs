using System;

namespace Bud.Tasking {
  public interface ITaskDefinition {
    string TaskName { get; }
    Type ReturnType { get; }
  }
}