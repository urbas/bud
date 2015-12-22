using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.Collections;
using Bud.IO;
using Bud.Optional;
using static System.Reactive.Linq.Observable;

namespace Bud.Reactive {
  public static class ObservableResources {
    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<T> resources,
                                                                  IObservable<T> observationTrigger)
      => Return(resources)
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

    public static IObservable<IEnumerable<T>> Combined<T>(this IEnumerable<IObservable<IEnumerable<T>>> watchedResources)
      => SingleEmptyResource<T>()
        .Concat(watchedResources)
        .CombineLatest(ImmutableLists.FlattenToImmutableList);

    public static IObservable<IEnumerable<T>> PileUp<T>(this IEnumerable<IObservable<T>> watchedResources)
      => ImmutableList.Create(Return(Optionals.None<T>()))
        .Concat(watchedResources.Select(observable => observable.Select(Optionals.Some)))
        .CombineLatest(OptionalExtensions.Gather);

    private static IEnumerable<IObservable<IEnumerable<T>>> SingleEmptyResource<T>()
      => EmptyResources<T>.SingleEmptyResourceImpl;

    private static class EmptyResources<T> {
      public static readonly IEnumerable<IObservable<IEnumerable<T>>> SingleEmptyResourceImpl =
        ImmutableList.Create(Return(Enumerable.Empty<T>()));
    }
  }
}