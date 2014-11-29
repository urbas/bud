using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Bud {

  public interface ISettingKey {}
  public interface IValuedKey : ISettingKey {}
  public interface IValuedKey<out T> : IValuedKey {}
  public interface IConfigKey : IValuedKey {}
  public interface IConfigKey<out T> : IConfigKey, IValuedKey<T> {}
  public interface ITaskKey : IValuedKey {}
  public interface ITaskKey<out T> : ITaskKey, IValuedKey<T> {}

  public class SettingKey : ISettingKey {
    public readonly string Id;
    public readonly ImmutableHashSet<ISettingKey> Scope;

    public SettingKey(string id) : this(id, ImmutableHashSet<ISettingKey>.Empty) {}

    public SettingKey(string id, ImmutableHashSet<ISettingKey> scope) {
      this.Id = id;
      this.Scope = scope;
    }

    public override bool Equals(object obj) {
      if (obj == null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (!(obj is ISettingKey))
        return false;
      SettingKey other = (SettingKey)obj;
      return Id.Equals(other.Id) && Scope.IsSubsetOf(other.Scope) && other.Scope.IsSubsetOf(Scope);
    }

    public override int GetHashCode() {
      unchecked {
        int hashCode = Id.GetHashCode();
        foreach (var settingKey in Scope) {
          hashCode ^= settingKey.GetHashCode();
        }
        return hashCode;
      }
    }

    public override string ToString() {
      if (Scope.IsEmpty)
        return Id;
      StringBuilder sb = new StringBuilder(Id).Append('{');
      var enumerator = Scope.GetEnumerator();
      enumerator.MoveNext();
      sb.Append(enumerator.Current);
      while (enumerator.MoveNext()) {
        sb.Append(", ").Append(enumerator.Current);
      }
      sb.Append('}');
      return sb.ToString();
    }
    
  }

  /// <summary>
  /// Values of this key are evaluated once only (during build loading).
  /// </summary>
  public class ConfigKey<T> : SettingKey, IValuedKey<T>, IConfigKey<T> {
    public ConfigKey(string id) : base(id) {}
    public ConfigKey(string id, ImmutableHashSet<ISettingKey> scope) : base(id, scope) {}
    public ConfigKey<T> In(ISettingKey scope) { return new ConfigKey<T>(Id, Scope.Add(scope)); }
  }

  /// <summary>
  /// Values of this key are evaluated once on every build evaluation (if two task keys A and B
  /// depend on the same task key C then the value of task key C will be calculated only once).
  /// </summary>
  public class TaskKey<T> : SettingKey, IValuedKey<T>, ITaskKey<T> {
    public TaskKey(string id) : base(id) {}
    public TaskKey(string id, ImmutableHashSet<ISettingKey> scope) : base(id, scope) {}
    public TaskKey<T> In(ISettingKey scope) { return new TaskKey<T>(Id, Scope.Add(scope)); }
  }

}

