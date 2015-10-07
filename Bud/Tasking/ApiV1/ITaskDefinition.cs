using System;

namespace Bud.Tasking.ApiV1 {
  public interface ITaskDefinition {
    Type ReturnType { get; }
    object Invoke(ITasks tasks);
  }
}