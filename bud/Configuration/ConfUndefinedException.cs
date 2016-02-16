using System;

namespace Bud.Configuration {
  public class ConfUndefinedException : Exception {
    public ConfUndefinedException(string message) : base(message) {}

    public ConfUndefinedException(string message, Exception exception) : base(message, exception) {}
  }
}