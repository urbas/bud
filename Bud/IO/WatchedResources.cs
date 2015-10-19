using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class WatchedResources<TResource> : IEnumerable<TResource> {
    public static readonly WatchedResources<TResource> Empty =
      new WatchedResources<TResource>(Enumerable.Empty<TResource>(),
                                      Observable.Empty<TResource>());

    public WatchedResources(IEnumerable<TResource> resources,
                            IObservable<TResource> resourceWatcher) {
      Resources = resources;
      Watcher = resourceWatcher;
    }

    public WatchedResources(WatchedResources<TResource> watchedResources)
      : this(watchedResources.Resources, watchedResources.Watcher) {}

    public IEnumerable<TResource> Resources { get; }
    public IObservable<TResource> Watcher { get; }
    public IEnumerator<TResource> GetEnumerator() => Resources.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public WatchedResources<TResource> ExpandWith(WatchedResources<TResource> other)
      => new WatchedResources<TResource>(Resources.Concat(other.Resources),
                                         Watcher.Merge(other.Watcher));

    public WatchedResources<TResource> WithFilter(Func<TResource, bool> filter)
      => new WatchedResources<TResource>(Resources.Where(filter),
                                         Watcher.Where(filter));

    public IObservable<WatchedResources<TResource>> Watch()
      => Observable.Return(this)
                   .Concat(Watcher.Select(_ => this));
  }
}