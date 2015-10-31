using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.CodeAnalysis;

namespace Bud.IO {
  public class WatchedResources<TResource> : IWatchedResources<TResource> {
    public static readonly WatchedResources<TResource> Empty =
      new WatchedResources<TResource>(Enumerable.Empty<TResource>(),
                                      Observable.Empty<TResource>());

    /// <param name="resources">
    ///   an enumeration of watched resources.
    ///   Must be enumerable multiple times and should
    ///   reflect changes that are reported by the watcher.
    /// </param>
    /// <param name="resourceWatcher">
    ///   a stream of changes to the enumeration of watched resources.
    /// </param>
    public WatchedResources(IEnumerable<TResource> resources,
                            IObservable<TResource> resourceWatcher) {
      Resources = resources;
      Watcher = resourceWatcher;
    }

    public WatchedResources(IWatchedResources<TResource> watchedResources)
      : this(watchedResources.Resources, watchedResources.Watcher) {}

    public IEnumerable<TResource> Resources { get; }
    public IObservable<TResource> Watcher { get; }
    public IEnumerator<TResource> GetEnumerator() => Resources.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public WatchedResources<TResource> ExpandWith(IWatchedResources<TResource> other)
      => new WatchedResources<TResource>(Resources.Concat(other.Resources),
                                         Watcher.Merge(other.Watcher));

    public WatchedResources<TResource> WithFilter(Func<TResource, bool> filter)
      => new WatchedResources<TResource>(Resources.Where(filter),
                                         Watcher.Where(filter));

    public IObservable<IWatchedResources<TResource>> Watch()
      => Observable.Return(this)
                   .Concat(Watcher.Select(_ => this));
  }

  public static class WatchedResources {
    public static IWatchedResources<T> WatchResource<T>(IObservable<T> observedValue) {
      return new WatchedSingletonResource<T>(observedValue);
    }

    internal class WatchedSingletonResource<T> : IWatchedResources<T> {
      private Optional<T> Subject { get; set; }
      public IEnumerable<T> Resources => this;
      public IObservable<T> Watcher { get; }

      public WatchedSingletonResource(IObservable<T> observedValue) {
        Watcher = observedValue;
      }

      public IEnumerator<T> GetEnumerator() {
        if (Subject.HasValue) {
          yield return Subject.Value;
        }
      }

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

      public IObservable<IWatchedResources<T>> Watch()
        => Watcher.Do(obj => Subject = obj).Select(_ => this);
    }
  }
}