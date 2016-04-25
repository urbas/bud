namespace Bud.V1 {
  /// <summary>
  ///   Keys are slash-separated paths.
  /// </summary>
  /// <typeparam name="T">the type of the value this key will point to.</typeparam>
  public struct Key<T> {
    public readonly string Id;

    public Key(string id) {
      Id = id;
    }

    public static implicit operator Key<T>(string id) => new Key<T>(id);
    public static implicit operator string(Key<T> key) => key.Id;

    /// <param name="conf">a bag of key-values.</param>
    /// <returns>
    ///   returns the value stored in the given <paramref name="conf" />
    ///   at the path pointed to by this key.
    /// </returns>
    public T this[IConf conf] => conf.Get(this);

    /// <param name="conf">a bag of key-values.</param>
    /// <returns>
    ///   returns the value stored in the given <paramref name="conf" />
    ///   at the path pointed to by this key.
    /// </returns>
    public T this[Conf conf] => conf.Get(this);

    /// <summary>
    ///   Joins the <paramref name="prefix" /> with the
    ///   <paramref name="key" /> separated with a slash character.
    /// </summary>
    /// <param name="prefix">a slash-separated path.</param>
    /// <param name="key">a slash-separated path.</param>
    /// <returns>a joined path.</returns>
    public static Key<T> operator /(string prefix, Key<T> key) => prefix + "/" + key.Id;

    /// <summary>
    ///   Joins the <paramref name="prefix" /> with the
    ///   <paramref name="key" /> separated with a slash character.
    /// </summary>
    /// <param name="prefix">a slash-separated path.</param>
    /// <param name="key">a slash-separated path.</param>
    /// <returns>a joined path.</returns>
    public static Key<T> operator /(Key prefix, Key<T> key) {
      if (Keys.Root.Equals(prefix)) {
        return Keys.SeparatorAsString + key.Id;
      }
      return prefix.Id + Keys.SeparatorAsString + key.Id;
    }

    public bool IsAbsolute => Keys.IsAbsolute(Id);
    public override string ToString() => Id;
  }

  public struct Key {
    public readonly string Id;

    public Key(string id) {
      Id = id;
    }

    /// <summary>
    ///   Joins the <paramref name="prefix" /> with the
    ///   <paramref name="key" /> separated with a slash character.
    /// </summary>
    /// <param name="prefix">a slash-separated path.</param>
    /// <param name="key">a slash-separated path.</param>
    /// <returns>a joined path.</returns>
    public static Key operator /(Key key, string prefix) {
      if (Keys.Root.Equals(key)) {
        return new Key(Keys.SeparatorAsString + prefix);
      }
      return new Key(key.Id + "/" + prefix);
    }

    public override string ToString() => Id;
  }
}