using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.Util {
  public struct Option<T> {
    public T Value { get; }
    public bool HasValue { get; }

    public Option(T value) {
      Value = value;
      HasValue = true;
    }

    public T GetOrElse(T defaultValue)
      => HasValue ? Value : defaultValue;

    public T GetOrElse(Func<T> defaultValue)
      => HasValue ? Value : defaultValue();

    public bool Equals(Option<T> other)
      => HasValue == other.HasValue
         && EqualityComparer<T>.Default.Equals(Value, other.Value);

    public override bool Equals(object obj)
      => !ReferenceEquals(null, obj)
      && obj is Option<T> 
      && Equals((Option<T>) obj);

    public override int GetHashCode() {
      unchecked {
        return (HasValue.GetHashCode()*397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
      }
    }

    public override string ToString() => HasValue ? $"Some({Value})" : "None";

    public static implicit operator Option<T>(T value)
      => new Option<T>(value);
  }

  public static class Option {
    public static Option<T> None<T>() => NoneOptional<T>.Instance;
    public static Option<T> Some<T>(T value) => new Option<T>(value);

    private static class NoneOptional<T> {
      public static readonly Option<T> Instance = new Option<T>();
    }

    public static IEnumerable<T> Gather<T>(this IEnumerable<Option<T>> enumerable)
      => enumerable.Where(optional => optional.HasValue)
                   .Select(optional => optional.Value);

    public static IEnumerable<TResult> Gather<TSource, TResult>(this IEnumerable<TSource> enumerable,
                                                                Func<TSource, Option<TResult>> selector)
      => enumerable.Select(selector).Gather();

    public static IObservable<T> Gather<T>(this IObservable<Option<T>> enumerable)
      => enumerable.Where(optional => optional.HasValue)
                   .Select(optional => optional.Value);

    public static IObservable<TResult> Gather<TSource, TResult>(this IObservable<TSource> enumerable,
                                                                Func<TSource, Option<TResult>> selector)
      => enumerable.Select(selector).Gather();
  }
}