using System;

namespace Bud {
  public struct Key {
    public readonly string Id;

    public Key(string id) {
      Id = id;
    }

    public static Key operator /(Key key, string prefix) => new Key(key.Id + "/" + prefix);
    public override string ToString() => Id;
  }

  public struct Key<T> {
    public readonly string Id;

    public Key(string id) {
      Id = id;
    }

    public static implicit operator Key<T>(string id) => new Key<T>(id);
    public static implicit operator string(Key<T> key) => key.Id;
    public T this[IConf conf] => conf.Get(this);
    public static Key<T> operator /(string prefix, Key<T> key) => prefix + "/" + key.Id;
    public static Key<T> operator /(Key prefix, Key<T> key) => prefix.Id + "/" + key.Id;
    public bool IsAbsolute => !string.IsNullOrEmpty(Id) && Id[0] == Keys.Separator;
    public override string ToString() => Id;

    public Conf SetValue(T value) => Conf.Empty.SetValue(this, value);
    public Conf InitValue(T value) => Conf.Empty.InitValue(this, value);
    public Conf Set(Func<IConf, T> value) => Conf.Empty.Set(this, value);
    public Conf Init(Func<IConf, T> value) => Conf.Empty.Init(this, value);
    public Conf Modify(Func<IConf, T, T> value) => Conf.Empty.Modify(this, value);
  }
}