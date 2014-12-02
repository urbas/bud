using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Bud.SettingsConstruction;
using Bud.Util;

namespace Bud {

  public interface ISettingKey {
    string Id { get; }

    Scope Scope { get; }

    SettingKey In(ISettingKey subScope);
  }

  public class SettingKey : ISettingKey {

    public Scope Scope  { get; private set; }

    public string Id { get { return Scope.Id; } }

    public SettingKey(string id) : this(Scope.Global.Add(id)) {
    }

    protected SettingKey(Scope scope) {
      this.Scope = scope;
    }

    public SettingKey In(ISettingKey subScope) {
      unchecked {
        return new SettingKey(CreateSubScope(subScope));
      }
    }

    public bool Equals(ISettingKey other) {
      return Scope.Equals(other.Scope);
    }

    public override bool Equals(object other) {
      if (other == null) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      if (!(other is ISettingKey)) {
        return false;
      }
      return Equals((ISettingKey)other);
    }

    public override int GetHashCode() {
      return Scope.GetHashCode();
    }

    public override string ToString() {
      return Scope.ToString();
    }

    protected Scope CreateSubScope(ISettingKey subScope) {
      return Scope.Parent.Add(subScope.Id).Add(Scope.Id);
    }
    
  }

  public interface ConfigKey : ISettingKey {
  }

  /// <summary>
  /// Values of this key are evaluated once only (during build loading).
  /// </summary>
  public class ConfigKey<T> : SettingKey, ConfigKey {
    public ConfigKey(string id) : base(id) {
    }

    private ConfigKey(Scope scope) : base(scope) {
    }

    SettingKey ISettingKey.In(ISettingKey subScope) {
      return In(subScope);
    }

    public new ConfigKey<T> In(ISettingKey subScope) {
      unchecked {
        return new ConfigKey<T>(CreateSubScope(subScope));
      }
    }
  }

  public interface TaskKey : ISettingKey {
  }

  /// <summary>
  /// Values of this key are evaluated once on every build evaluation (if two task keys A and B
  /// depend on the same task key C then the value of task key C will be calculated only once).
  /// </summary>
  public class TaskKey<T> : SettingKey, TaskKey {
    public TaskKey(string id) : base(id) {
    }

    private TaskKey(Scope scope) : base(scope) {
    }

    SettingKey ISettingKey.In(ISettingKey subScope) {
      return In(subScope);
    }

    public new TaskKey<T> In(ISettingKey subScope) {
      unchecked {
        return new TaskKey<T>(CreateSubScope(subScope));
      }
    }
  }
}

