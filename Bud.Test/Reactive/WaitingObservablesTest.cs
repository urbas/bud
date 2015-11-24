using System;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using static System.Reactive.Linq.Observable;
using static System.TimeSpan;
using static NUnit.Framework.Assert;

namespace Bud.Reactive {
  public class WaitingObservablesTest {
    private TestScheduler scheduler;
    private IObservable<long> tenTickInterval;

    [SetUp]
    public void SetUp() {
      scheduler = new TestScheduler();
      tenTickInterval = Interval(FromTicks(10), scheduler);
    }

    [Test]
    public void SkipUntilCalm_produces_empty_observables_from_empty_observables()
      => IsEmpty(Empty<int>().SkipUntilCalm(FromMilliseconds(100)).ToEnumerable());

    [Test]
    public void SkipUntilCalm_produces_the_last_element_when_observable_finishes_before_calming_period() {
      var observable = tenTickInterval.Take(5)
                                      .SkipUntilCalm(FromTicks(100), scheduler)
                                      .GetEnumerator();
      scheduler.AdvanceBy(9001);
      IsTrue(observable.MoveNext());
      AreEqual(4, observable.Current);
    }

    [Test]
    public void SkipUntilCalm_produces_the_last_element_when_observable_produces_faster_than_calming_period() {
      var observable = tenTickInterval.Take(15)
                                      .SkipUntilCalm(FromTicks(50), scheduler)
                                      .GetEnumerator();
      scheduler.AdvanceBy(9001);
      IsTrue(observable.MoveNext());
      AreEqual(14, observable.Current);
    }

    [Test]
    public void SkipUntilCalm_produces_skips_some_elements() {
      var observable = tenTickInterval.Take(3)
                                      .Merge(tenTickInterval.Select(l => l + 10).Take(2).Delay(FromTicks(100), scheduler))
                                      .Merge(tenTickInterval.Select(l => l + 20).Take(1).Delay(FromTicks(200), scheduler))
                                      .SkipUntilCalm(FromTicks(20), scheduler)
                                      .GetEnumerator();
      scheduler.AdvanceBy(9001);
      IsTrue(observable.MoveNext());
      AreEqual(2, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(11, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(20, observable.Current);
      IsFalse(observable.MoveNext());
    }

    [Test]
    public void SkipUntilCalm_skips_nothing_when_production_interval_is_shorter_than_calming_period() {
      var observable = tenTickInterval.SkipUntilCalm(FromTicks(1), scheduler)
                                      .Take(3)
                                      .GetEnumerator();
      scheduler.AdvanceBy(9001);
      IsTrue(observable.MoveNext());
      AreEqual(0, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(1, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(2, observable.Current);
      IsFalse(observable.MoveNext());
    }
  }
}