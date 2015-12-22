using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.CodeAnalysis;

namespace Bud.Optional {
  public static class OptionalExtensions {
    public static T GetOrElse<T>(this Optional<T> optional, T defaultValue)
      => optional.HasValue ? optional.Value : defaultValue;

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