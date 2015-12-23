using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Bud.IO {
  public class Watched<T> {
    /// <param name="value">
    ///   See <see cref="Value" />
    /// </param>
    /// <param name="changes">
    ///   See <see cref="Changes" />.
    /// </param>
    public Watched(T value, IObservable<T> changes) {
      Value = value;
      Changes = changes;
    }

    /// <summary>
    ///   The watched value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    ///   A stream of notifications of <see cref="Value" /> changes.
    /// </summary>
    /// <remarks>
    ///   Every observation of this observable contains information about what
    ///   parts of resources have changed. For example, if the type of <see cref="Value" />
    ///   is a collection, then this watcher produces collections of changed elements.
    /// </remarks>
    public IObservable<T> Changes { get; }
  }

  public static class Watched {
    public static Watched<T> Watch<T>(T value, IObservable<T> watcher)
      => new Watched<T>(value, watcher);

    /// <returns>
    ///   a watched value that triggers no changes.
    /// </returns>
    public static Watched<T> Watch<T>(T value)
      => new Watched<T>(value, Observable.Empty<T>());

    /// <returns>
    ///   a watched collection of values that triggers no changes.
    /// </returns>
    public static Watched<IEnumerable<T>> Watch<T>(params T[] values)
      => Watch<IEnumerable<T>>(values);
  }
}