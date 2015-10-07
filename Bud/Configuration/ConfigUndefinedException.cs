using System;

namespace Bud.Configuration {
  public class ConfigUndefinedException : Exception {
    public ConfigUndefinedException(string message) : base(message) {}

    public ConfigUndefinedException(string message, Exception exception) : base(message, exception) {}
  }
}