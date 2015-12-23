using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.Collections;
using Bud.IO;
using Bud.Optional;
using Microsoft.CodeAnalysis;
using static System.Reactive.Linq.Observable;

namespace Bud.Reactive {
  public static class ObservableResources {
    public static IObservable<T> ObserveResource<T>(T resource,
                                                    IObservable<T> changes)
      => Return(resource).Concat(changes.Select(_ => resource));

    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<T> resources,
                                                                  IObservable<IEnumerable<T>> changes)
      => Return(resources).Concat(changes.Select(_ => resources));

    public static IObservable<IEnumerable<TValue>> ObserveResources<TValue, TChange>(IEnumerable<TValue> resources,
                                                                                     IObservable<TChange> changes)
      => Return(resources).Concat(changes.Select(_ => resources));

    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<T> resources,
                                                                  IObservable<IEnumerable<T>> observationTrigger,
                                                                  Func<T, bool> filter)
      => ObserveResources(resources.Where(filter),
                          observationTrigger.Where(e => e.Any(filter)));

    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<Watched<IEnumerable<T>>> watchedResources,
                                                                  Func<T, bool> filter)
      => ObserveResources(watchedResources.SelectMany(resources => resources.Value),
                          watchedResources.Select(resources => resources.Changes).Merge(),
                          filter);

    public static IObservable<IEnumerable<T>> ObserveResources<T>(params Watched<T>[] watchedResources)
      => ObserveResources((IEnumerable<Watched<T>>) watchedResources);

    public static IObservable<IEnumerable<T>> ObserveResources<T>(IEnumerable<Watched<T>> watchedResources) {
      var resources = watchedResources.Select(watched => watched.Value);
      var changes = watchedResources.Select(watched => watched.Changes)
                                    .Merge();
      return ObserveResources(resources, changes);
    }

    public static IObservable<IEnumerable<T>> Combined<T>(this IEnumerable<IObservable<IEnumerable<T>>> watchedResources)
      => SingleEmptyResource<T>()
        .Concat(watchedResources)
        .CombineLatest(ImmutableLists.FlattenToImmutableList);

    public static IObservable<IEnumerable<T>> PileUp<T>(this IEnumerable<IObservable<T>> watchedResources)
      => ImmutableList.Create(Return(Optionals.None<T>()))
                      .Concat(watchedResources.Select(observable => observable.Select<T, Optional<T>>(Optionals.Some)))
                      .CombineLatest(OptionalExtensions.Gather);

    private static IEnumerable<IObservable<IEnumerable<T>>> SingleEmptyResource<T>()
      => EmptyResources<T>.SingleEmptyResourceImpl;

    private static class EmptyResources<T> {
      public static readonly IEnumerable<IObservable<IEnumerable<T>>> SingleEmptyResourceImpl =
        ImmutableList.Create(Return(Enumerable.Empty<T>()));
    }
  }
}