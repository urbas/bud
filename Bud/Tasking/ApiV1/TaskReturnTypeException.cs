using System;

namespace Bud.Tasking.ApiV1 {
  public class TaskReturnTypeException : Exception {
    public TaskReturnTypeException(string message) : base(message) {}
  }
}