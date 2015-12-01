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
    public bool IsAbsolute => Keys.IsAbsolute(Id);
    public override string ToString() => Id;
  }
}