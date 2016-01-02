using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.Util {
  public struct Optional<T> {
    public T Value { get; }
    public bool HasValue { get; }

    public Optional(T value) {
      Value = value;
      HasValue = true;
    }

    public bool Equals(Optional<T> other)
      => HasValue == other.HasValue
         && EqualityComparer<T>.Default.Equals(Value, other.Value);

    public override bool Equals(object obj)
      => !ReferenceEquals(null, obj)
      && obj is Optional<T> 
      && Equals((Optional<T>) obj);

    public override int GetHashCode() {
      unchecked {
        return (HasValue.GetHashCode()*397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
      }
    }

    public override string ToString() => HasValue ? $"Some({Value})" : "None";

    public static implicit operator Optional<T>(T value)
      => new Optional<T>(value);
  }

  public static class Optional {
    public static Optional<T> None<T>() => NoneOptional<T>.Instance;
    public static Optional<T> Some<T>(T value) => new Optional<T>(value);

    private static class NoneOptional<T> {
      public static readonly Optional<T> Instance = new Optional<T>();
    }

    public static T GetOrElse<T>(this Optional<T> optional, T defaultValue)
      => optional.HasValue ? optional.Value : defaultValue;

    public static T GetOrElse<T>(this Optional<T> optional, Func<T> defaultValue)
      => optional.HasValue ? optional.Value : defaultValue();

    public static IEnumerable<T> Gather<T>(this IEnumerable<Optional<T>> enumerable)
      => enumerable.Where(optional => optional.HasValue)
                   .Select(optional => optional.Value);

    public static IEnumerable<TResult> Gather<TSource, TResult>(this IEnumerable<TSource> enumerable,
                                                                Func<TSource, Optional<TResult>> selector)
      => enumerable.Select(selector).Gather();

    public static IObservable<T> Gather<T>(this IObservable<Optional<T>> enumerable)
      => enumerable.Where(optional => optional.HasValue)
                   .Select(optional => optional.Value);

    public static IObservable<TResult> Gather<TSource, TResult>(this IObservable<TSource> enumerable,
                                                                Func<TSource, Optional<TResult>> selector)
      => enumerable.Select(selector).Gather();
  }
}