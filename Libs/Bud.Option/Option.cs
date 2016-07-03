using System;
using System.Collections.Generic;
using System.Linq;

namespace Bud {
  public struct Option<T> {
    public T Value { get; }
    public bool HasValue { get; }

    public Option(T value) {
      Value = value;
      HasValue = true;
    }

    public T GetOrElse(T defaultValue) => HasValue ? Value : defaultValue;
    public T GetOrElse(Func<T> defaultValue) => HasValue ? Value : defaultValue();
    public static implicit operator Option<T>(T value) => new Option<T>(value);
    public Option<T> OrElse(T defaultValue) => HasValue ? this : defaultValue;
    public Option<T> OrElse(Option<T> defaultValue) => HasValue ? this : defaultValue;
    public Option<T> OrElse(Func<T> defaultValue) => HasValue ? this : defaultValue();
    public Option<T> OrElse(Func<Option<T>> defaultValue) => HasValue ? this : defaultValue();

    public Option<TResult> Map<TResult>(Func<T, TResult> mapFunc)
      => HasValue ? mapFunc(Value) : Option.None<TResult>();

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

    public override string ToString()
      => HasValue ?
           $"Some<{GetType().GetGenericArguments()[0]}>({Value})" :
           $"None<{GetType().GetGenericArguments()[0]}>";
  }

  public static class Option {
    public static Option<T> None<T>() => NoneOptional<T>.Instance;
    public static Option<T> Some<T>(T value) => new Option<T>(value);

    private static class NoneOptional<T> {
      public static readonly Option<T> Instance = new Option<T>();
    }

    public static Option<T> Flatten<T>(this Option<Option<T>> nestedOption)
      => nestedOption.HasValue ? nestedOption.Value : None<T>();

    public static IEnumerable<T> Gather<T>(this IEnumerable<Option<T>> enumerable)
      => enumerable.Where(optional => optional.HasValue)
                   .Select(optional => optional.Value);

    public static IEnumerable<TResult> Gather<TSource, TResult>(this IEnumerable<TSource> enumerable,
                                                                Func<TSource, Option<TResult>> selector)
      => enumerable.Select(selector).Gather();

    public static Option<TValue> Get<TKey, TValue>(this IDictionary<TKey, TValue> dict,
                                                   TKey key) {
      TValue value;
      return dict.TryGetValue(key, out value) ? value : None<TValue>();
    }
  }
}