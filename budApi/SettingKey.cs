using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Bud.SettingsConstruction;
using Bud.Util;

namespace Bud {

  public interface ISettingKey {
    string Id { get; }

    ImmutableList<ISettingKey> Scope { get; }

    SettingKey In(ISettingKey subScope);
  }

  public interface IValuedKey : ISettingKey {

  }

  public interface IValuedKey<out T> : IValuedKey {

  }

  public interface ITaskKey : IValuedKey {

  }

  public class SettingKey : ISettingKey {

    protected readonly int hash;

    public string Id { get; private set; }

    public ImmutableList<ISettingKey> Scope  { get; private set; }

    public SettingKey(string id) : this(id, ImmutableList<ISettingKey>.Empty, id.GetHashCode()) {
    }

    protected SettingKey(string id, ImmutableList<ISettingKey> scope, int hash) {
      this.Id = id;
      this.Scope = scope;
      this.hash = hash;
    }

    public SettingKey In(ISettingKey subScope) {
      unchecked {
        return new SettingKey(Id, Scope.Add(subScope), hash ^ subScope.GetHashCode());
      }
    }

    public override bool Equals(object obj) {
      if (obj == null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (!(obj is ISettingKey))
        return false;
      ISettingKey other = (ISettingKey)obj;
      return Id.Equals(other.Id) && ScopeUtils.AreEqual(Scope, other.Scope);
    }

    public override int GetHashCode() {
        return hash;
    }

    public override string ToString() {
      return SettingKeyUtils.KeyAsString(this);
    }
    
  }

  /// <summary>
  /// Values of this key are evaluated once only (during build loading).
  /// </summary>
  public class ConfigKey<T> : SettingKey, IValuedKey<T> {
    public ConfigKey(string id) : base(id) {
    }

    private ConfigKey(string id, ImmutableList<ISettingKey> scope, int hash) : base(id, scope, hash) {
    }

    SettingKey ISettingKey.In(ISettingKey subScope) {
      return In(subScope);
    }

    public new ConfigKey<T> In(ISettingKey subScope) {
      unchecked {
        return new ConfigKey<T>(Id, Scope.Add(subScope), hash ^ subScope.GetHashCode());
      }
    }
  }

  /// <summary>
  /// Values of this key are evaluated once on every build evaluation (if two task keys A and B
  /// depend on the same task key C then the value of task key C will be calculated only once).
  /// </summary>
  public class TaskKey<T> : SettingKey, IValuedKey<T>, ITaskKey {
    public TaskKey(string id) : base(id) {
    }

    private TaskKey(string id, ImmutableList<ISettingKey> scope, int hash) : base(id, scope, hash) {
    }

    SettingKey ISettingKey.In(ISettingKey subScope) {
      return In(subScope);
    }

    public new TaskKey<T> In(ISettingKey subScope) {
      unchecked {
        return new TaskKey<T>(Id, Scope.Add(subScope), hash ^ subScope.GetHashCode());
      }
    }
  }
}

