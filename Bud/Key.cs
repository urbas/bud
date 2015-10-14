namespace Bud {
  public struct Key<T> {
    public readonly string Id;

    public Key(string id) {
      Id = id;
    }

    public static implicit operator Key<T>(string id) => new Key<T>(id);

    public static implicit operator string(Key<T> key) => key.Id;

    public T this[IConf conf] => conf.Get(this);

    public static Key<T> operator /(string prefix, Key<T> key) => prefix + "/" + key.Id;
  }
}