using System.Collections.Generic;
using System.IO;

namespace Bud.IO {
  public struct Hashed<T> : IHashed {
    public Hashed(T value, long hash) {
      Hash = hash;
      Value = value;
    }

    public long Hash { get; }
    public T Value { get; }

    public bool Equals(Hashed<T> other)
      => EqualityComparer<T>.Default.Equals(Value, other.Value);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      return (obj is Hashed<T> && Equals((Hashed<T>) obj)) || (obj is T && Value.Equals((T)obj));
    }

    public override int GetHashCode()
      => EqualityComparer<T>.Default.GetHashCode(Value);

    public static bool operator ==(Hashed<T> left, Hashed<T> right)
      => left.Equals(right);

    public static bool operator !=(Hashed<T> left, Hashed<T> right)
      => !left.Equals(right);

    public static bool operator ==(Hashed<T> left, T right)
      => left.Value.Equals(right);

    public static bool operator !=(Hashed<T> left, T right)
      => !left.Value.Equals(right);

    public override string ToString()
      => $"Hashed(Value: {Value}, Hash: {Hash})";
  }

  public static class Hashed {
    public static Hashed<TValue> Create<TValue>(TValue value, long hash)
      => new Hashed<TValue>(value, hash);

    public static long GetTimeHash(string file)
      => File.GetLastWriteTime(file).ToFileTime();
  }
}