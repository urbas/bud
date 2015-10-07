using System;

namespace Bud.Configuration {
  public class ConfigDefinitionException : Exception {
    public string Key { get; }
    public Type ValueType { get; }

    public ConfigDefinitionException(string key, Type valueType, string message) : base(message) {
      Key = key;
      ValueType = valueType;
    }
  }
}