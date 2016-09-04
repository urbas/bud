using System;
using System.Reactive.Linq;

namespace Bud.Reactive {
  public static class ObservableOptions {
    /// <summary>
    ///   This method discards from the original observable all options without values.
    ///   Options with values are not discarded. Their contained values are  passed to the resulting observable.
    /// </summary>
    /// <typeparam name="T">the type of elements contained in the option instances.</typeparam>
    /// <param name="observable">the original observable. Produces option instances.</param>
    /// <returns>
    ///   an observable producing elements contained in option instances.
    /// </returns>
    public static IObservable<T> Gather<T>(this IObservable<Option<T>> observable)
      => observable.Where(optional => optional.HasValue)
                   .Select(optional => optional.Value);

    /// <summary>
    ///   This method applies the given <paramref name="selector" /> function on each element of the original observable.
    ///   The selector function returns an option. This option is discarded if it contains no value. Otherwise the element
    ///   contained in the option is passed on to the resulting observable.
    /// </summary>
    /// <typeparam name="TSource">the type of elements contained in the original observable.</typeparam>
    /// <typeparam name="TResult">
    ///   the type of elements that will end up in the resulting observable. This is also the type of
    ///   options produced by the <paramref name="selector" /> function.
    /// </typeparam>
    /// <param name="observable">the original observable on whose elements to apply the <paramref name="selector" />.</param>
    /// <param name="selector">
    ///   this function is applied on each element of the original <paramref name="observable" />.
    /// </param>
    /// <returns>
    ///   a new observable that contains elements contained within options produced by the <paramref name="selector" />
    ///   function.
    /// </returns>
    public static IObservable<TResult> Gather<TSource, TResult>(this IObservable<TSource> observable,
                                                                Func<TSource, Option<TResult>> selector)
      => observable.Select(selector).Gather();
  }
}