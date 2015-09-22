using System;

namespace Bud.Tasking {
  public class TaskNotDefinedException : Exception {
    public TaskNotDefinedException(string message) : base(message) {}
  }
}