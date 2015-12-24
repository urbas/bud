using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class Watcher<T> {
    /// <param name="value">
    ///   See <see cref="Value" />
    /// </param>
    /// <param name="changes">
    ///   See <see cref="Changes" />.
    /// </param>
    public Watcher(T value, IObservable<T> changes) {
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

  public static class Watcher {
    public static Watcher<T> Watch<T>(T value)
      => new Watcher<T>(value, Observable.Empty<T>());

    public static Watcher<T> Watch<T>(T value, IObservable<T> changes)
      => new Watcher<T>(value, changes);

    public static Watcher<IEnumerable<T>> WatchMany<T>(params T[] value)
      => new Watcher<IEnumerable<T>>(value, Observable.Empty<IEnumerable<T>>());

    public static IObservable<TValue> ObserveResource<TValue, TChange>(TValue value,
                                                                       IObservable<TChange> changes)
      => Observable.Return(value).Concat(changes.Select(_ => value));

    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<T> values,
                                                                  IObservable<IEnumerable<T>> observationTrigger,
                                                                  Func<T, bool> filter)
      => ObserveResource(values.Where(filter),
                         observationTrigger.Where(e => e.Any<T>(filter)));

    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<Watcher<IEnumerable<T>>> watchers,
                                                                  Func<T, bool> filter)
      => ObserveResources(watchers.SelectMany(resources => resources.Value),
                          watchers.Select(resources => resources.Changes).Merge(),
                          filter);

    public static IObservable<IEnumerable<T>> ObserveResources<T>(params Watcher<T>[] watchers)
      => ObserveResources((IEnumerable<Watcher<T>>) watchers);

    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<Watcher<T>> watchers) {
      var values = watchers.Any() ? watchers.Select(watched => watched.Value) : Enumerable.Empty<T>();
      var changes = watchers.Select(watched => watched.Changes)
                            .Merge();
      return ObserveResource(values, changes);
    }
  }
}