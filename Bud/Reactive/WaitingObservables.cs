using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.CodeAnalysis;

namespace Bud.Reactive {
  public static class WaitingObservables {
    public static IObservable<T> SkipUntilCalm<T>(this IObservable<T> observable,
                                                  TimeSpan calmingPeriod)
      => SkipUntilCalm(observable, calmingPeriod, DefaultScheduler.Instance);

    public static IObservable<T> SkipUntilCalm<T>(this IObservable<T> observable,
                                                  TimeSpan calmingPeriod,
                                                  IScheduler scheduler) {
      var shared = observable.Publish().RefCount();
      return shared.Window(CalmingWindows(shared, calmingPeriod, scheduler))
                   .SelectMany(o => o.Aggregate(new Optional<T>(), (element, nextElement) => new Optional<T>(nextElement)))
                   .Where(optional => optional.HasValue)
                   .Select(optional => optional.Value);
    }

    public static IObservable<T> CalmAfterFirst<T>(this IObservable<T> observableToThrottle,
                                                   TimeSpan calmingPeriod,
                                                   IScheduler scheduler) {
      var sharedInput = observableToThrottle.Publish().RefCount();
      return sharedInput.Take(1)
                        .Concat(sharedInput.Skip(1).SkipUntilCalm(calmingPeriod, scheduler));
    }

    private static IObservable<Unit> CalmingWindows<T>(IObservable<T> observable, TimeSpan calmingPeriod, IScheduler scheduler)
      => Observable.Create<Unit>(observer => {
        var timerDisposable = new MultipleAssignmentDisposable();
        var calmingWindows = new Subject<Unit>();
        IDisposable localTimerDisposable = null;
        var calmerSubscription = observable
          .Subscribe(next => {
            localTimerDisposable?.Dispose();
            localTimerDisposable = scheduler
              .Schedule(calmingPeriod, () => calmingWindows.OnNext(Unit.Default));
            timerDisposable.Disposable = localTimerDisposable;
          }, exception => {
            timerDisposable.Dispose();
            calmingWindows.OnError(exception);
          }, () => {
            timerDisposable.Dispose();
            calmingWindows.OnCompleted();
          });
        return new CompositeDisposable {
          timerDisposable,
          calmerSubscription,
          calmingWindows.Subscribe(observer),
          calmingWindows
        };
      });
  }
}