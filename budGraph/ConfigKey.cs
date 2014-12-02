using System;

namespace Bud {
  /// <summary>
  /// Values of this key are evaluated once only per settings compilation.
  /// </summary>
  public class ConfigKey<T> : Scope {
    public ConfigKey(string id) : base(id) {
    }

    private ConfigKey(string id, Scope parent) : base(id, parent) {
    }

    public new ConfigKey<T> In(Scope parent) {
      if (Parent.Equals(parent)) {
        return this;
      }
      return new ConfigKey<T>(Id, parent);
    }
  }
}

