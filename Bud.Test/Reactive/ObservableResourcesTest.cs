using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using static System.Reactive.Linq.Observable;
using static Bud.IO.Watched;
using static Bud.Reactive.ObservableResources;
using static NUnit.Framework.Assert;

namespace Bud.Reactive {
  public class ObservableResourcesTest {
    [Test]
    public void ObserveResources_produces_the_first_observation()
      => AreEqual(new[] {1},
                  ObserveResource(1, Empty<int>()).ToList().Wait());

    [Test]
    public void ObserveResources_produces_consecutive_observations()
      => AreEqual(new[] {1, 1},
                  ObserveResource(1, Return(42)).ToList().Wait());

    [Test]
    public void ObserveResources_does_not_subscribe_to_the_watcher_if_not_observing_past_first_notification() {
      var resourceWatcher = new Mock<IObservable<int>>(MockBehavior.Strict);
      ObserveResource(1, resourceWatcher.Object).ToEnumerable().First();
      resourceWatcher.VerifyAll();
    }

    [Test]
    public void ObserveResources_filters() {
      var watched = Watch<IEnumerable<int>>(new[] {2, 3, 4},
                                            new[] {new[] {1, 2, 3}}.ToObservable());
      var observable = ObserveResources(new[] {watched}, n => n%2 == 0);
      var observations = observable.ToList().Wait();
      AreEqual(2, observations.Count);
      That(observations, Is.All.EqualTo(new[] {2, 4}));
    }

    [Test]
    public void Observing_two_empty_watched_resources_triggers_a_single_observation() {
      var observable = ObserveResources(Watch(1), Watch(2));
      AreEqual(new[] {new[] {1, 2}},
               observable.ToList().Wait());
    }

    [Test]
    public void Observing_two_watched_resources_merges_observation_triggers() {
      var watchedResources1 = Watch(1, Return(1));
      var watchedResources2 = Watch(2, Return(2));
      var observations = ObserveResources(watchedResources1, watchedResources2).ToList().Wait();
      AreEqual(3, observations.Count);
      That(observations, Is.All.EqualTo(new[] {1, 2}));
    }

    [Test]
    public void Combined_produces_an_empty_observation()
      => AreEqual(new[] {Enumerable.Empty<int>()},
                  Enumerable.Empty<IObservable<IEnumerable<int>>>().Combined().ToList().Wait());

    [Test]
    public void Combined_produces_concatenated_observations() {
      var scheduler = new TestScheduler();
      var actual = new[] {Return(new[] {1, 2}).Concat(Return(new[] {3, 4}).Delay(TimeSpan.FromTicks(10), scheduler)), Return(new[] {42})}
        .Combined().GetEnumerator();
      IsTrue(actual.MoveNext());
      AreEqual(new[] {1, 2, 42}, actual.Current);
      scheduler.AdvanceBy(10);
      IsTrue(actual.MoveNext());
      AreEqual(new[] {3, 4, 42}, actual.Current);
      scheduler.AdvanceBy(1);
      IsFalse(actual.MoveNext());
    }

    [Test]
    public void PileUp_produces_an_empty_observation()
      => AreEqual(new[] {Enumerable.Empty<int>()},
                  Enumerable.Empty<IObservable<int>>().PileUp().ToList().Wait());

    [Test]
    public void PileUp_produces_concatenated_observations() {
      var scheduler = new TestScheduler();
      var actual = new[] {Return(1).Concat(Return(2).Delay(TimeSpan.FromTicks(10), scheduler)), Return(42)}
        .PileUp().GetEnumerator();
      IsTrue(actual.MoveNext());
      AreEqual(new[] {1, 42}, actual.Current);
      scheduler.AdvanceBy(10);
      IsTrue(actual.MoveNext());
      AreEqual(new[] {2, 42}, actual.Current);
      scheduler.AdvanceBy(1);
      IsFalse(actual.MoveNext());
    }
  }
}