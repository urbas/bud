using System;

namespace Bud.Tasking {
  public class TaskUndefinedException : Exception {
    public TaskUndefinedException(string message) : base(message) {}
  }
}