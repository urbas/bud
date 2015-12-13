using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;

namespace Bud.Reactive {
  public static class ObservableResources {
    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<T> resources,
                                                                  IObservable<T> observationTrigger)
      => Observable.Return(resources)
                   .Concat(observationTrigger.Select(_ => resources));

    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<T> resources,
                                                                  IObservable<T> observationTrigger,
                                                                  Func<T, bool> filter)
      => ObserveResources(resources.Where(filter),
                          observationTrigger.Where(filter));

    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<Watched<T>> watchedResources,
                                                                  Func<T, bool> filter)
      => ObserveResources(watchedResources.SelectMany(resources => resources.Resources),
                          watchedResources.Select(resources => resources.Watcher).Merge(),
                          filter);

    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<Watched<T>> watchedResources)
      => ObserveResources(watchedResources, _ => true);
  }
}