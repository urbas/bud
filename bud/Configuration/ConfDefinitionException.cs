using System;

namespace Bud.Configuration {
  public class ConfDefinitionException : Exception {
    public string Key { get; }
    public Type ValueType { get; }

    public ConfDefinitionException(string key, Type valueType, string message) : base(message) {
      Key = key;
      ValueType = valueType;
    }
  }
}