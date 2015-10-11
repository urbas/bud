using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.Pipeline {
  /// <summary>
  ///   A stream is an observable that produces collections.
  /// </summary>
  public static class Streams {
    /// <returns>an empty stream. The empty stream produces a single empty collection.</returns>
    public static IObservable<IEnumerable<T>> Empty<T>() => EmptyStream<T>.Instance;

    /// <returns>
    ///   a stream that produces concatenated collections from streams
    ///   <paramref name="stream" /> and <paramref name="otherStream" />.
    /// </returns>
    public static IObservable<IEnumerable<T>> CombineStream<T>(this IObservable<IEnumerable<T>> stream, IObservable<IEnumerable<T>> otherStream)
      => stream.CombineLatest(otherStream, (collection, otherCollection) => collection.Concat(otherCollection));

    /// <returns>a stream that produces a single collection.</returns>
    public static IObservable<IEnumerable<T>> CreateStream<T>(params T[] collection)
      => Observable.Return((IEnumerable<T>) collection);

    private static class EmptyStream<T> {
      public static readonly IObservable<IEnumerable<T>> Instance = Observable.Return(Enumerable.Empty<T>());
    }
  }
}