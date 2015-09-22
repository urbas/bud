using System;

namespace Bud.Tasking {
  public class TaskReturnsDifferentTypeException : Exception {
    public TaskReturnsDifferentTypeException(string message) : base(message) {}
  }
}