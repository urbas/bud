using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static Bud.Option;

namespace Bud.Reactive {
  public static class ObservableCalming {
    /// <summary>
    ///   Transforms the given <paramref name="observable" /> into a new observable. The new observable
    ///   waits until some time (given by the <paramref name="calmingPeriod" /> parameter) has passed from the
    ///   last-produced element in the original obervable. This last element will then end up in the
    ///   new observable. After this, the new observable waits for a new element from the original observable
    ///   and repeats the procedure.
    /// </summary>
    /// <typeparam name="T">
    ///   the type of elements in the returned and given <paramref name="observable" />.
    /// </typeparam>
    /// <param name="observable">the observable from which to construct a calmed observable.</param>
    /// <param name="calmingPeriod">
    ///   the amount of time to wait from the last-produced element in
    ///   the given <paramref name="observable" />.
    /// </param>
    /// <param name="scheduler">
    ///   The scheduler that will be used to determine calming windows. This parameter
    ///   is optional, by default <see cref="DefaultScheduler.Instance" /> is used.
    /// </param>
    /// <returns>
    ///   a new observable that produces only those elements from the given <paramref name="observable" />
    ///   after which there were no elements for the amount of time given by the <paramref name="calmingPeriod" />
    ///   parameter.
    /// </returns>
    public static IObservable<T> Calmed<T>(this IObservable<T> observable,
                                           TimeSpan calmingPeriod,
                                           IScheduler scheduler = null) {
      var shared = observable.Publish().RefCount();
      scheduler = scheduler ?? DefaultScheduler.Instance;
      return shared.Window(CalmingWindows(shared, calmingPeriod, scheduler))
                   .SelectMany(o => o.Aggregate(None<T>(), (element, nextElement) => Some(nextElement)))
                   .Gather();
    }

    /// <summary>
    ///   This method is similar to <see cref="Calmed{T}" /> with one difference:
    ///   the observable returned by this method immeditately returns the first element that comes from the
    ///   given <paramref name="observable" />. Only after that it starts behaving the same as <see cref="Calmed{T}" />.
    /// </summary>
    /// <typeparam name="T">
    ///   the type of elements in the returned and given <paramref name="observable" />.
    /// </typeparam>
    /// <param name="observable">the observable from which to construct a calmed observable.</param>
    /// <param name="calmingPeriod">
    ///   the amount of time to wait from the last-produced element in
    ///   the given <paramref name="observable" />.
    /// </param>
    /// <param name="scheduler">
    ///   The scheduler that will be used to determine calming windows. This parameter
    ///   is optional, by default <see cref="DefaultScheduler.Instance" /> is used.
    /// </param>
    /// <returns>
    ///   a new observable that produces the first element from the given <paramref name="observable" /> and afterwards
    ///   those elements from the given <paramref name="observable" /> after which there were no elements for the amount
    ///   of time given by the <paramref name="calmingPeriod" /> parameter.
    /// </returns>
    public static IObservable<T> FirstThenCalmed<T>(this IObservable<T> observable,
                                                    TimeSpan calmingPeriod,
                                                    IScheduler scheduler = null) {
      var sharedInput = observable.Publish().RefCount();
      return sharedInput.Take(1)
                        .Concat(sharedInput.Skip(1).Calmed(calmingPeriod, scheduler));
    }

    /// <summary>
    ///   Transforms the given <paramref name="observable" /> into a new observable. The new observable
    ///   collects all elements produced by the given observable until it hits an element, after which
    ///   some time (given by the <paramref name="calmingPeriod" /> parameter) has passed without any
    ///   newly produced elements. At this point, the new observable will produce an element that is a
    ///   collection of all the elements collected so far from the original observable.
    ///   After this, the new observable waits for a new element from the original observable
    ///   and repeats the procedure.
    /// </summary>
    /// <typeparam name="T">
    ///   the type of elements in the returned and given <paramref name="observable" />.
    /// </typeparam>
    /// <param name="observable">the observable from which to construct a calmed observable.</param>
    /// <param name="calmingPeriod">
    ///   the amount of time to wait from the last-produced element in
    ///   the given <paramref name="observable" />.
    /// </param>
    /// <param name="scheduler">
    ///   The scheduler that will be used to determine calming windows. This parameter
    ///   is optional, by default <see cref="DefaultScheduler.Instance" /> is used.
    /// </param>
    /// <returns>
    ///   a new observable that produces a collection of elements collected from the given <paramref name="observable" />
    ///   during the time that it wated for the first such element after which there were no elements for the
    ///   duration given by the <paramref name="calmingPeriod" /> parameter.
    /// </returns>
    public static IObservable<ImmutableArray<T>> CalmedBatches<T>(this IObservable<T> observable,
                                                                  TimeSpan calmingPeriod,
                                                                  IScheduler scheduler = null) {
      var shared = observable.Publish().RefCount();
      scheduler = scheduler ?? DefaultScheduler.Instance;
      return shared.Window(CalmingWindows(shared, calmingPeriod, scheduler))
                   .SelectMany(o => o.Aggregate(ImmutableArray.CreateBuilder<T>(),
                                                (collectedSoFar, nextElement) => {
                                                  collectedSoFar.Add(nextElement);
                                                  return collectedSoFar;
                                                }))
                   .Where(list => list.Count > 0)
                   .Select(builder => builder.ToImmutable());
    }

    [SuppressMessage("ReSharper", "ArgumentsStyleAnonymousFunction")]
    private static IObservable<Unit> CalmingWindows<T>(IObservable<T> observable,
                                                       TimeSpan calmingPeriod,
                                                       IScheduler scheduler)
      => Observable.Create<Unit>(observer => {
        var calmingTimer = new MultipleAssignmentDisposable();
        var calmingWindows = new Subject<Unit>();
        var calmerSubscription = observable.Subscribe(
          onNext: next => {
            calmingTimer.Disposable?.Dispose();
            calmingTimer.Disposable = scheduler.Schedule(
              calmingPeriod,
              () => calmingWindows.OnNext(Unit.Default));
          },
          onError: exception => {
            calmingTimer.Dispose();
            calmingWindows.OnError(exception);
          },
          onCompleted: () => {
            calmingTimer.Dispose();
            calmingWindows.OnCompleted();
          });
        return new CompositeDisposable {
          calmingTimer,
          calmerSubscription,
          calmingWindows.Subscribe(observer),
          calmingWindows
        };
      });
  }
}