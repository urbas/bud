using System;
using System.Collections.Generic;
using System.IO;

namespace Bud.IO {
  public struct Timestamped<T> : ITimestamped {
    public Timestamped(T value, DateTimeOffset timestamp) {
      Timestamp = timestamp;
      Value = value;
    }

    public DateTimeOffset Timestamp { get; }
    public T Value { get; }

    public bool Equals(Timestamped<T> other)
      => EqualityComparer<T>.Default.Equals(Value, other.Value);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      return (obj is Timestamped<T> && Equals((Timestamped<T>) obj)) || (obj is T && Value.Equals((T)obj));
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
      => $"Timestamped(Value: {Value}, {Timestamp})";
  }

  public static class Timestamped {
    public static Timestamped<TValue> Create<TValue>(TValue value, DateTimeOffset timestamp)
      => new Timestamped<TValue>(value, timestamp);

    public static Timestamped<string> CreateFromFile(string path)
      => new Timestamped<string>(path, File.GetLastWriteTime(path));
  }
}