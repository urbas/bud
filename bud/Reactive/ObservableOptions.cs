using System;
using System.Reactive.Linq;

namespace Bud.Reactive {
  public static class ObservableOptions {
    public static IObservable<T> Gather<T>(this IObservable<Option<T>> enumerable)
      => enumerable.Where(optional => optional.HasValue)
                   .Select(optional => optional.Value);

    public static IObservable<TResult> Gather<TSource, TResult>(this IObservable<TSource> enumerable,
                                                                Func<TSource, Option<TResult>> selector)
      => enumerable.Select(selector).Gather();
  }
}