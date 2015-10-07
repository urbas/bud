using System;

namespace Bud.Configuration {
  static internal class ConfigAssertions {
    internal static void RequireConfigType<T>(string configKey, Type configType) {
      if (configType != typeof(T)) {
        throw new ConfigTypeException($"Expected configuration '{configKey}' to be of type '{configType}'. Its actual type is '{typeof(T)}'.");
      }
    }
  }
}