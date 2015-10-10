using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.Pipeline {
  /// <summary>
  ///   A pipe is an observable that produces collections.
  /// </summary>
  public static class Pipes {
    /// <returns>a pipe that produces a single empty collection.</returns>
    public static IObservable<IEnumerable<T>> Empty<T>() => SingleEmptyCollectionPipe<T>.Instance;

    /// <returns>
    ///   a pipe that produces entries made by concatenating the last collections from the pipes
    ///   <paramref name="pipe" /> and <paramref name="otherPipe" />.
    /// </returns>
    public static IObservable<IEnumerable<T>> JoinPipes<T>(this IObservable<IEnumerable<T>> pipe, IObservable<IEnumerable<T>> otherPipe)
      => pipe.CombineLatest(otherPipe, (oldFiles, newFiles) => oldFiles.Concat(newFiles));

    /// <returns>
    ///   a pipe that produces the same entries as <paramref name="pipe" /> but with a 'Where(<paramref name="predicate" />)'
    ///   filter applied
    ///   on each of them.
    /// </returns>
    public static IObservable<IEnumerable<T>> FilterPipe<T>(this IObservable<IEnumerable<T>> pipe, Func<T, bool> predicate)
      => pipe.Select(enumerable => enumerable.Where(predicate));

    /// <returns>a pipe that produces a single collection.</returns>
    public static IObservable<IEnumerable<T>> ToPipe<T>(IEnumerable<T> collection)
      => Observable.Return(collection);

    /// <returns>a pipe that produces a single collection.</returns>
    public static IObservable<IEnumerable<T>> CreatePipe<T>(params T[] collection)
      => ToPipe(collection);

    /// <returns>a pipe that produces a single collection.</returns>
    /// <remarks>the <paramref name="collectionFactory" /> method is invoked lazily whenever a new observer pulls.</remarks>
    public static IObservable<IEnumerable<T>> ToPipe<T>(Func<IEnumerable<T>> collectionFactory)
      => Observable.Create<IEnumerable<T>>(observer => ToPipe(collectionFactory()).Subscribe(observer));

    private static class SingleEmptyCollectionPipe<T> {
      public static readonly IObservable<IEnumerable<T>> Instance = Observable.Return(Enumerable.Empty<T>());
    }
  }
}