using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class WatchedResources<TResource> : IEnumerable<TResource> {
    public static readonly WatchedResources<TResource> Empty =
      new WatchedResources<TResource>(
        Enumerable.Empty<TResource>,
        Observable.Empty<object>);

    public WatchedResources(Func<IEnumerable<TResource>> inputFactory,
                            Func<IObservable<object>> watcherFactory) {
      InputFactory = inputFactory;
      WatcherFactory = watcherFactory;
    }

    public WatchedResources(WatchedResources<TResource> watchedResources)
      : this(watchedResources.InputFactory, watchedResources.WatcherFactory) {}

    public Func<IEnumerable<TResource>> InputFactory { get; }
    public Func<IObservable<object>> WatcherFactory { get; }
    public IEnumerator<TResource> GetEnumerator() => InputFactory().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public WatchedResources<TResource> ExpandWith(WatchedResources<TResource> other)
      => new WatchedResources<TResource>(() => InputFactory().Concat(other.InputFactory()),
                                         () => WatcherFactory().Merge(other.WatcherFactory()));

    public WatchedResources<TResource> WithFilter(Func<TResource, bool> filter)
      => new WatchedResources<TResource>(() => InputFactory().Where(filter),
                                         WatcherFactory);

    public IObservable<WatchedResources<TResource>> Watch()
      => Observable.Return(this)
                   .Concat(WatcherFactory().Select(_ => this));
  }
}