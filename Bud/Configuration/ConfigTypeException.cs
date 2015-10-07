using System;

namespace Bud.Configuration {
  public class ConfigTypeException : Exception {
    public ConfigTypeException(string message) : base(message) {}
  }
}