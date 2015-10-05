using System;

namespace Bud.Tasking {
  public class TaskReturnTypeException : Exception {
    public TaskReturnTypeException(string message) : base(message) {}
  }
}