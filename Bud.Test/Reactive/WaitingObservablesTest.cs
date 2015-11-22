using System;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace Bud.Reactive {
  public class WaitingObservablesTest {
    private TestScheduler scheduler;
    private IObservable<long> tenTickInterval;

    [SetUp]
    public void SetUp() {
      scheduler = new TestScheduler();
      tenTickInterval = Observable.Interval(TimeSpan.FromTicks(10), scheduler);
    }

    [Test]
    public void SkipUntilCalm_produces_empty_observables_from_empty_observables()
      => Assert.IsEmpty(Observable.Empty<int>().SkipUntilCalm(TimeSpan.FromMilliseconds(100)).ToEnumerable());

    [Test]
    public void SkipUntilCalm_produces_the_last_element_when_observable_finishes_before_calming_period() {
      var observable = tenTickInterval.Take(5)
                                      .SkipUntilCalm(TimeSpan.FromTicks(100), scheduler)
                                      .GetEnumerator();
      scheduler.AdvanceBy(9001);
      Assert.IsTrue(observable.MoveNext());
      Assert.AreEqual(4, observable.Current);
    }

    [Test]
    public void SkipUntilCalm_produces_the_last_element_when_observable_produces_faster_than_calming_period() {
      var observable = tenTickInterval.Take(15)
                                      .SkipUntilCalm(TimeSpan.FromTicks(50), scheduler)
                                      .GetEnumerator();
      scheduler.AdvanceBy(9001);
      Assert.IsTrue(observable.MoveNext());
      Assert.AreEqual(14, observable.Current);
    }

    [Test]
    public void SkipUntilCalm_produces_skips_some_elements() {
      var observable = tenTickInterval.Take(3)
                                      .Merge(tenTickInterval.Select(l => l + 10).Take(2).Delay(TimeSpan.FromTicks(100), scheduler))
                                      .Merge(tenTickInterval.Select(l => l + 20).Take(1).Delay(TimeSpan.FromTicks(200), scheduler))
                                      .SkipUntilCalm(TimeSpan.FromTicks(20), scheduler)
                                      .GetEnumerator();
      scheduler.AdvanceBy(9001);
      Assert.IsTrue(observable.MoveNext());
      Assert.AreEqual(2, observable.Current);
      Assert.IsTrue(observable.MoveNext());
      Assert.AreEqual(11, observable.Current);
      Assert.IsTrue(observable.MoveNext());
      Assert.AreEqual(20, observable.Current);
      Assert.IsFalse(observable.MoveNext());
    }

    [Test]
    public void SkipUntilCalm_skips_nothing_when_production_interval_is_shorter_than_calming_period() {
      var observable = tenTickInterval.SkipUntilCalm(TimeSpan.FromTicks(1), scheduler)
                                      .Take(3)
                                      .GetEnumerator();
      scheduler.AdvanceBy(9001);
      Assert.IsTrue(observable.MoveNext());
      Assert.AreEqual(0, observable.Current);
      Assert.IsTrue(observable.MoveNext());
      Assert.AreEqual(1, observable.Current);
      Assert.IsTrue(observable.MoveNext());
      Assert.AreEqual(2, observable.Current);
      Assert.IsFalse(observable.MoveNext());
    }
  }
}