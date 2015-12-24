using System;
using System.Collections.Generic;
using System.Linq;
using Bud.Collections;
using static System.Reactive.Linq.Observable;

namespace Bud.Reactive {
  public static class ObservableResources {
    /// <param name="observables">
    ///   Each observable in this collection must produce at least one element.
    /// </param>
    /// <remarks>
    ///   If <paramref name="observables" /> is empty, this method produces an
    ///   observable with a single empty enumeration.
    /// </remarks>
    public static IObservable<IEnumerable<T>> Combined<T>(this IEnumerable<IObservable<IEnumerable<T>>> observables)
      => observables.Any() ?
        observables.CombineLatest(ImmutableLists.FlattenToImmutableList) :
        Return(Enumerable.Empty<T>());

    /// <param name="observables">
    ///   Each observable in this collection must produce at least one element.
    /// </param>
    /// <remarks>
    ///   If <paramref name="observables" /> is empty, this method produces an
    ///   observable with a single empty enumeration.
    /// </remarks>
    public static IObservable<IEnumerable<T>> Combined<T>(this IEnumerable<IObservable<T>> observables)
      => observables.Any() ? observables.CombineLatest() : Return(Enumerable.Empty<T>());
  }
}