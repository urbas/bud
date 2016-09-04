using System;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using static System.Reactive.Linq.Observable;
using static System.TimeSpan;
using static NUnit.Framework.Assert;

namespace Bud.Reactive {
  public class ObservableCalmingTest {
    private TestScheduler scheduler;
    private IObservable<long> tenTickInterval;

    [SetUp]
    public void SetUp() {
      scheduler = new TestScheduler();
      tenTickInterval = Interval(FromTicks(10), scheduler);
    }

    [Test]
    public void Calmed_produces_empty_observables_from_empty_observables()
      => IsEmpty(Empty<int>().Calmed(FromMilliseconds(100)).ToEnumerable());

    [Test]
    public void Calmed_produces_the_last_element_when_observable_finishes_before_calming_period() {
      var observable = tenTickInterval.Take(5)
                                      .Calmed(FromTicks(100), scheduler)
                                      .GetEnumerator();
      scheduler.AdvanceBy(9001);
      IsTrue(observable.MoveNext());
      AreEqual(4, observable.Current);
    }

    [Test]
    public void Calmed_produces_the_last_element_when_observable_produces_faster_than_calming_period() {
      var observable = tenTickInterval.Take(15)
                                      .Calmed(FromTicks(50), scheduler)
                                      .GetEnumerator();
      scheduler.AdvanceBy(9001);
      IsTrue(observable.MoveNext());
      AreEqual(14, observable.Current);
    }

    [Test]
    public void Calmed_skips_some_elements() {
      var observable = ThreeBursts().Calmed(FromTicks(20), scheduler).GetEnumerator();
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
    public void Calmed_skips_nothing_when_production_interval_is_shorter_than_calming_period() {
      var observable = tenTickInterval.Calmed(FromTicks(1), scheduler)
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

    [Test]
    public void Calmed_stops_producing_elements_on_exception() {
      var observable = Return("foo", scheduler)
        .Concat(Return("bar", scheduler).Delay(FromTicks(200), scheduler))
        .Concat(Throw<string>(new Exception("foobar"), scheduler))
        .Calmed(FromTicks(150), scheduler)
        .GetEnumerator();

      // |-o--------------------o-----------------o--------o-|
      //   ^                    ^                 ^        ^
      //   foo                  calmed            bar      exception
      //   t = 1                t = 151           t = 203  t = 204

      scheduler.AdvanceTo(151);
      observable.MoveNext();
      AreEqual("foo", observable.Current);

      scheduler.AdvanceTo(204);
      var exception = Throws<Exception>(() => observable.MoveNext());
      AreEqual("foobar", exception.Message);
      IsNull(observable.Current);
    }

    [Test]
    public void FirstThenCalmed_returns_empty()
      => IsEmpty(Empty<int>().FirstThenCalmed(FromTicks(100), scheduler).ToEnumerable());

    [Test]
    public void FirstThenCalmed_produces_the_first_observation_when_next_happens_within_calming_period() {
      var enumerator = tenTickInterval.Take(5).FirstThenCalmed(FromTicks(100), scheduler).GetEnumerator();
      scheduler.AdvanceBy(50);
      IsTrue(enumerator.MoveNext());
      AreEqual(0, enumerator.Current);
    }

    [Test]
    public void FirstThenCalmed_produces_the_second_observation_when_after_calming_period() {
      var observable = ThreeBursts().FirstThenCalmed(FromTicks(20), scheduler).GetEnumerator();
      scheduler.AdvanceBy(9001);
      IsTrue(observable.MoveNext());
      AreEqual(0, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(2, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(11, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(20, observable.Current);
      IsFalse(observable.MoveNext());
    }

    [Test]
    public void CalmedBatches_collects_single_elements_when_calming_period_too_short() {
      var observable = ThreeBursts().CalmedBatches(FromTicks(1), scheduler).GetEnumerator();
      scheduler.AdvanceBy(9001);
      IsTrue(observable.MoveNext());
      AreEqual(new [] {0}, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(new [] {1}, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(new [] {2}, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(new [] {10}, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(new [] {11}, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(new [] {20}, observable.Current);
      IsFalse(observable.MoveNext());
    }

    [Test]
    public void CalmedBatches_collects_groups() {
      var observable = ThreeBursts().CalmedBatches(FromTicks(50), scheduler).GetEnumerator();
      scheduler.AdvanceBy(9001);
      IsTrue(observable.MoveNext());
      AreEqual(new [] {0, 1, 2}, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(new [] {10, 11}, observable.Current);
      IsTrue(observable.MoveNext());
      AreEqual(new [] {20}, observable.Current);
      IsFalse(observable.MoveNext());
    }

    private IObservable<long> ThreeBursts()
      => tenTickInterval.Take(3)
                        .Merge(tenTickInterval.Select(l => l + 10).Take(2).Delay(FromTicks(100), scheduler))
                        .Merge(tenTickInterval.Select(l => l + 20).Take(1).Delay(FromTicks(200), scheduler));
  }
}