using System;
using System.Collections.Generic;
using System.Linq;
using Bud.IO;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using static System.Reactive.Linq.Observable;
using static NUnit.Framework.Assert;

namespace Bud.Reactive {
  public class ObservableResourcesTest {
    [Test]
    public void ObserveResources_produces_the_first_observation() {
      var observable = ObservableResources.ObserveResources(Enumerable.Empty<int>(),
                                                            Empty<int>());
      AreEqual(new[] {Enumerable.Empty<int>()},
               observable.ToList().Wait());
    }

    [Test]
    public void ObserveResources_produces_consecutive_observations() {
      var observable = ObservableResources.ObserveResources(Enumerable.Empty<int>(),
                                                            Return(42));
      AreEqual(new[] {Enumerable.Empty<int>(), Enumerable.Empty<int>()},
               observable.ToList().Wait());
    }

    [Test]
    public void ObserveResources_does_not_subscribe_to_the_watcher_if_not_observing_past_first_notification() {
      var resourceWatcher = new Mock<IObservable<int>>(MockBehavior.Strict);
      ObservableResources.ObserveResources(Enumerable.Empty<int>(), resourceWatcher.Object).ToEnumerable().First();
      resourceWatcher.VerifyAll();
    }

    [Test]
    public void ObserveResources_filters_observations() {
      var observable = ObservableResources.ObserveResources(Enumerable.Empty<int>(),
                                                            new[] {1, 2, 3}.ToObservable(),
                                                            n => n%2 == 0);
      var observations = observable.ToList().Wait();
      AreEqual(2, observations.Count);
      That(observations, Is.All.EqualTo(Enumerable.Empty<int>()));
    }

    [Test]
    public void ObserveResources_filters_resources() {
      var observable = ObservableResources.ObserveResources(new[] {2, 3, 4},
                                                            new[] {1, 2, 3}.ToObservable(),
                                                            n => n%2 == 0);
      That(observable.ToList().Wait(),
           Is.All.EqualTo(new[] {2, 4}));
    }

    [Test]
    public void Observing_two_empty_watched_resources_triggers_a_single_empty_observation() {
      var observable = ObservableResources.ObserveResources(new[] {Watched<int>.Empty, Watched<int>.Empty});
      AreEqual(new[] {Enumerable.Empty<int>()},
               observable.ToList().Wait());
    }

    [Test]
    public void Observing_two_watched_resources_merges_their_elements() {
      var watchedResources1 = new Watched<int>(new[] {42}, Empty<int>());
      var watchedResources2 = new Watched<int>(new[] {9001}, Empty<int>());
      var observable = ObservableResources.ObserveResources(new[] {watchedResources1, watchedResources2});
      AreEqual(new[] {new[] {42, 9001}},
               observable.ToList().Wait());
    }

    [Test]
    public void Observing_two_watched_resources_merges_observation_triggers() {
      var watchedResources1 = new Watched<int>(Enumerable.Empty<int>(), Return(1));
      var watchedResources2 = new Watched<int>(Enumerable.Empty<int>(), Return(2));
      var observations = ObservableResources.ObserveResources(new[] {watchedResources1, watchedResources2}).ToList().Wait();
      AreEqual(3, observations.Count);
      That(observations, Is.All.EqualTo(Enumerable.Empty<int>()));
    }

    [Test]
    public void Observing_two_watched_resources_filters_their_elements() {
      var watchedResources1 = new Watched<int>(new[] {42}, Empty<int>());
      var watchedResources2 = new Watched<int>(new[] {9001}, Empty<int>());
      var observable = ObservableResources.ObserveResources(new[] {watchedResources1, watchedResources2}, i => 42 == i);
      AreEqual(new[] {new[] {42}},
               observable.ToList().Wait());
    }

    [Test]
    public void Observing_two_watched_resources_filters_their_elements_and_observation_triggers() {
      var watchedResources1 = new Watched<int>(new[] {42}, Return(42));
      var watchedResources2 = new Watched<int>(new[] {9001}, Return(9001));
      var observable = ObservableResources.ObserveResources(new[] {watchedResources1, watchedResources2}, i => 42 == i);
      AreEqual(new[] {new[] {42}, new[] {42}},
               observable.ToList().Wait());
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
  }
}