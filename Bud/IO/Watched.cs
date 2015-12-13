using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class Watched<TResource> {
    public static Watched<TResource> Empty { get; } = new Watched<TResource>(Enumerable.Empty<TResource>(), Observable.Empty<TResource>());

    /// <param name="resources">
    ///   an enumeration of watched resources.
    ///   Must be enumerable multiple times and should
    ///   reflect changes that are reported by the watcher.
    /// </param>
    /// <param name="resourceWatcher">
    ///   a stream of changes to the enumeration of watched resources.
    /// </param>
    public Watched(IEnumerable<TResource> resources,
                   IObservable<TResource> resourceWatcher) {
      Resources = resources;
      Watcher = resourceWatcher;
    }

    public IEnumerable<TResource> Resources { get; }
    public IObservable<TResource> Watcher { get; }
  }

  public static class Watched {
    public static Watched<T> Watch<T>(IEnumerable<T> resources, IObservable<T> observationTrigger)
      => new Watched<T>(resources, observationTrigger);

    public static Watched<T> Watch<T>(IEnumerable<T> resources)
      => new Watched<T>(resources, Observable.Empty<T>());
  }
}