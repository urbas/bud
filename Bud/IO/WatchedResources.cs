using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class WatchedResources<TResource> : IWatchedResources<TResource> {
    public static readonly WatchedResources<TResource> Empty =
      new WatchedResources<TResource>(Enumerable.Empty<TResource>(),
                                      Observable.Empty<TResource>());

    /// <param name="lister">
    ///   an enumeration of watched resources.
    ///   Must be enumerable multiple times and should
    ///   reflect changes that are reported by the watcher.
    /// </param>
    /// <param name="resourceWatcher">
    ///   a stream of changes to the enumeration of watched resources.
    /// </param>
    public WatchedResources(IEnumerable<TResource> lister,
                            IObservable<TResource> resourceWatcher) {
      Lister = lister;
      Watcher = resourceWatcher;
    }

    public WatchedResources(IWatchedResources<TResource> watchedResources)
      : this(watchedResources.Lister, watchedResources.Watcher) {}

    public IEnumerable<TResource> Lister { get; }
    public IObservable<TResource> Watcher { get; }

    public WatchedResources<TResource> ExpandWith(IWatchedResources<TResource> other)
      => new WatchedResources<TResource>(Lister.Concat(other.Lister),
                                         Watcher.Merge(other.Watcher));

    public WatchedResources<TResource> WithFilter(Func<TResource, bool> filter)
      => new WatchedResources<TResource>(Lister.Where(filter),
                                         Watcher.Where(filter));
  }

  public static class WatchedResources {
    /// <remarks>
    ///   The returned observable will immediately produce an initial observation.
    ///   After the first observation, more will be triggered by the watcher
    ///   in <paramref name="watchedResources" />.
    /// </remarks>
    public static IObservable<IEnumerable<TResource>> Watch<TResource>(this IWatchedResources<TResource> watchedResources)
      => Observable
        .Return(watchedResources.Lister)
        .Concat(watchedResources.Watcher.Select(_ => watchedResources.Lister));
  }
}