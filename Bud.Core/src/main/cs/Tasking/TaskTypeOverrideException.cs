using System;

namespace Bud.Tasking {
  public class TaskTypeOverrideException : Exception {
    public TaskTypeOverrideException(string message) : base(message) {}
  }
}