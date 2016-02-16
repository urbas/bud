using System.Collections.Generic;
using System.Linq;

namespace Bud.IO {
  public struct Timestamped<T> : ITimestamped {
    public Timestamped(T value, long timestamp) {
      Timestamp = timestamp;
      Value = value;
    }

    public long Timestamp { get; }
    public T Value { get; }

    public bool Equals(Timestamped<T> other)
      => EqualityComparer<T>.Default.Equals(Value, other.Value);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      return (obj is Timestamped<T> && Equals((Timestamped<T>) obj)) || (obj is T && Value.Equals((T) obj));
    }

    public override int GetHashCode()
      => EqualityComparer<T>.Default.GetHashCode(Value);

    public static bool operator ==(Timestamped<T> left, Timestamped<T> right)
      => left.Equals(right);

    public static bool operator !=(Timestamped<T> left, Timestamped<T> right)
      => !left.Equals(right);

    public static bool operator ==(Timestamped<T> left, T right)
      => left.Value.Equals(right);

    public static bool operator !=(Timestamped<T> left, T right)
      => !left.Value.Equals(right);

    public override string ToString()
      => $"Timestamped(Value: {Value}, Timestamp: {Timestamp})";

    public static implicit operator T(Timestamped<T> timestamped)
      => timestamped.Value;
  }

  public static class Timestamped {
    public static Timestamped<TValue> Create<TValue>(TValue value, long hash)
      => new Timestamped<TValue>(value, hash);

    public static bool IsUpToDateWith<T1, T2>(this Timestamped<T1> timestamped, Timestamped<T2> other)
      => timestamped.Timestamp >= other.Timestamp;

    public static bool IsUpToDateWith<T1, T2>(this Timestamped<T1> timestamped, IEnumerable<Timestamped<T2>> other)
      => other.All(timestamped1 => timestamped.IsUpToDateWith(timestamped1));
  }
}