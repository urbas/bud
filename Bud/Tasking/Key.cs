using System;
using System.Threading.Tasks;

namespace Bud.Tasking {
  public struct Key {
    public readonly string Id;

    public Key(string id) {
      Id = id;
    }

    public static implicit operator Key(string id) => new Key(id);

    public static implicit operator string(Key key) => key.Id;

    public Task this[ITasks tasks] => tasks.Get(this);

    public static Key operator /(string prefix, Key key) => prefix + "/" + key.Id;
  }

  public struct Key<T> {
    public readonly string Id;

    public Key(string id) {
      Id = id;
    }

    public static implicit operator Key<T>(string id) => new Key<T>(id);

    public static implicit operator string(Key<T> key) => key.Id;

    public static implicit operator Key(Key<T> key) => new Key(key.Id);

    public Task<T> this[ITasks tasks] => tasks.Get(this);

    public static Key<T> operator /(string prefix, Key<T> key) => prefix + "/" + key.Id;

    public Type Type => typeof(T);
  }
}