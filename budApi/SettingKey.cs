using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Bud {

  public interface ISettingKey {
    ISettingKey In(ISettingKey subScope);
    string Id { get; }
    ISettingKey Scope { get; }
  }

  public interface IValuedKey : ISettingKey {

  }

  public interface IValuedKey<out T> : IValuedKey {

  }

  public interface ITaskKey : IValuedKey {

  }

  public class SettingKey : ISettingKey {
    public string Id { get; private set; }
    public ISettingKey Scope  { get; private set; }

    public SettingKey(string id) : this(id, GlobalScope.Instance) {}

    public SettingKey(string id, ISettingKey scope) {
      this.Id = id;
      this.Scope = scope;
    }

    ISettingKey ISettingKey.In(ISettingKey subScope) {
      return new SettingKey(Id, subScope.In(Scope));
    }

    public override bool Equals(object obj) {
      if (obj == null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (!(obj is ISettingKey))
        return false;
      ISettingKey other = (ISettingKey)obj;
      return Id.Equals(other.Id) && Scope.Equals(other.Scope);
    }

    public override int GetHashCode() {
      unchecked {
        return Id.GetHashCode() ^ Scope.GetHashCode();
      }
    }

    public override string ToString() {
      return Scope.ToString() + ":" + Id;
    }
    
  }

  /// <summary>
  /// Values of this key are evaluated once only (during build loading).
  /// </summary>
  public class ConfigKey<T> : SettingKey, IValuedKey<T> {
    public ConfigKey(string id) : base(id) {
    }

    public ConfigKey(string id, ISettingKey scope) : base(id, scope) {
    }

    ISettingKey ISettingKey.In(ISettingKey subScope) {
      return In(subScope);
    }

    public ConfigKey<T> In(ISettingKey subScope) {
      return new ConfigKey<T>(Id, Scope.In(subScope));
    }
  }

  /// <summary>
  /// Values of this key are evaluated once on every build evaluation (if two task keys A and B
  /// depend on the same task key C then the value of task key C will be calculated only once).
  /// </summary>
  public class TaskKey<T> : SettingKey, IValuedKey<T>, ITaskKey {
    public TaskKey(string id) : base(id) {
    }

    public TaskKey(string id, ISettingKey scope) : base(id, scope) {
    }

    ISettingKey ISettingKey.In(ISettingKey subScope) {
      return In(subScope);
    }

    public TaskKey<T> In(ISettingKey subScope) {
      return new TaskKey<T>(Id, Scope.In(subScope));
    }
  }

  public sealed class GlobalScope : ISettingKey {

    public static readonly ISettingKey Instance = new GlobalScope();

    private GlobalScope() {}

    public string Id { get { return "GlobalScope"; } }

    public ISettingKey Scope { get { return null; } }

    public ISettingKey In(ISettingKey subScope) {
      return new SettingKey(Id, subScope);
    }

    public override bool Equals(object obj) {
      if (obj == null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return obj.GetType() == typeof(GlobalScope);
    }


    public override int GetHashCode() {
      return Id.GetHashCode();
    }
  }
}

