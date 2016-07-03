using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using static System.Reactive.Linq.Observable;
using static NUnit.Framework.Assert;

namespace Bud.Reactive {
  public class ObservableResourcesTest {
    [Test]
    public void Combined_enumerations_produces_an_empty_observation()
      => AreEqual(new[] {Enumerable.Empty<int>()},
                  Enumerable.Empty<IObservable<IEnumerable<int>>>().Combined().ToList().Wait());

    [Test]
    public void Combined_enumerations_produces_concatenated_observations() {
      var scheduler = new TestScheduler();
      var actual = new[] {
        Return(new[] {1, 2}).Concat(Return(new[] {3, 4}).Delay(TimeSpan.FromTicks(10), scheduler)),
        Return(new[] {42})
      }
        .Combined<int>().GetEnumerator();
      IsTrue(actual.MoveNext());
      AreEqual(new[] {1, 2, 42}, actual.Current);
      scheduler.AdvanceBy(10);
      IsTrue(actual.MoveNext());
      AreEqual(new[] {3, 4, 42}, actual.Current);
      scheduler.AdvanceBy(1);
      IsFalse(actual.MoveNext());
    }

    [Test]
    public void Combined_produces_an_empty_observation()
      => AreEqual(new[] {Enumerable.Empty<int>()},
                  Enumerable.Empty<IObservable<int>>().Combined().ToList().Wait());

    [Test]
    public void Combined_produces_concatenated_observations() {
      var scheduler = new TestScheduler();
      var piledUp = new[] {
        Return(1).Concat(Return(2).Delay(TimeSpan.FromTicks(10), scheduler)),
        Return(42)
      }
        .Combined().GetEnumerator();
      IsTrue(piledUp.MoveNext());
      AreEqual(new[] {1, 42}, piledUp.Current);
      scheduler.AdvanceBy(10);
      IsTrue(piledUp.MoveNext());
      AreEqual(new[] {2, 42}, piledUp.Current);
      scheduler.AdvanceBy(1);
      IsFalse(piledUp.MoveNext());
    }
  }
}