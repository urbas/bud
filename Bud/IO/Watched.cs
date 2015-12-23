using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class Watched<T> {
    public static Watched<T> Empty { get; }
      = new Watched<T>(Enumerable.Empty<T>(),
                       Observable.Empty<T>());

    /// <param name="resources">
    ///   an enumeration of watched resources.
    ///   Must be enumerable multiple times and should
    ///   reflect changes that are reported by the watcher.
    /// </param>
    /// <param name="watcher">
    ///   a stream of notifications on when an element in <paramref name="resources" />
    ///   has changed.
    /// </param>
    public Watched(IEnumerable<T> resources,
                   IObservable<T> watcher) {
      Resources = resources;
      Watcher = watcher;
    }

    public IEnumerable<T> Resources { get; }
    public IObservable<T> Watcher { get; }
  }

  public static class Watched {
    public static Watched<T> Watch<T>(IEnumerable<T> resources,
                                      IObservable<T> watcher)
      => new Watched<T>(resources, watcher);

    public static Watched<T> Watch<T>(IEnumerable<T> resources)
      => new Watched<T>(resources, Observable.Empty<T>());

    public static Watched<T> Watch<T>(params T[] resources)
      => Watch((IEnumerable<T>) resources);
  }
}