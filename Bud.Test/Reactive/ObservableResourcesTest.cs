using System;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Moq;
using NUnit.Framework;

namespace Bud.Reactive {
  public class ObservableResourcesTest {
    [Test]
    public void ObserveResources_produces_the_first_observation() {
      var observable = ObservableResources.ObserveResources(Enumerable.Empty<int>(),
                                                            Observable.Empty<int>());
      Assert.AreEqual(new[] {Enumerable.Empty<int>()},
                      observable.ToList().Wait());
    }

    [Test]
    public void ObserveResources_produces_consecutive_observations() {
      var observable = ObservableResources.ObserveResources(Enumerable.Empty<int>(),
                                                            Observable.Return(42));
      Assert.AreEqual(new[] {Enumerable.Empty<int>(), Enumerable.Empty<int>()},
                      observable.ToList().Wait());
    }

    [Test]
    public void ObserveResources_does_not_subscribe_to_the_watcher_if_not_observing_past_first_notification()
    {
      var resourceWatcher = new Mock<IObservable<int>>(MockBehavior.Strict);
      ObservableResources.ObserveResources(Enumerable.Empty<int>(), resourceWatcher.Object).ToEnumerable().First();
      resourceWatcher.VerifyAll();
    }

    [Test]
    public void ObserveResources_filters_observations() {
      var observable = ObservableResources.ObserveResources(Enumerable.Empty<int>(),
                                                            new[] {1, 2, 3}.ToObservable(),
                                                            n => n % 2 == 0);
      var observations = observable.ToList().Wait();
      Assert.AreEqual(2, observations.Count);
      Assert.That(observations, Is.All.EqualTo(Enumerable.Empty<int>()));
    }

    [Test]
    public void ObserveResources_filters_resources() {
      var observable = ObservableResources.ObserveResources(new[] {2, 3, 4},
                                                            new[] {1, 2, 3}.ToObservable(),
                                                            n => n % 2 == 0);
      Assert.That(observable.ToList().Wait(),
                  Is.All.EqualTo(new[] {2, 4}));
    }

    [Test]
    public void Observing_two_empty_watched_resources_triggers_a_single_empty_observation() {
      var observable = ObservableResources.ObserveResources(new[] {Watched<int>.Empty, Watched<int>.Empty});
      Assert.AreEqual(new[] { Enumerable.Empty<int>() },
                      observable.ToList().Wait());
    }

    [Test]
    public void Observing_two_watched_resources_merges_their_elements()
    {
      var watchedResources1 = new Watched<int>(new []{42}, Observable.Empty<int>());
      var watchedResources2 = new Watched<int>(new []{9001}, Observable.Empty<int>());
      var observable = ObservableResources.ObserveResources(new[] {watchedResources1, watchedResources2});
      Assert.AreEqual(new[] { new [] {42, 9001} },
                      observable.ToList().Wait());
    }

    [Test]
    public void Observing_two_watched_resources_merges_observation_triggers()
    {
      var watchedResources1 = new Watched<int>(Enumerable.Empty<int>(), Observable.Return(1));
      var watchedResources2 = new Watched<int>(Enumerable.Empty<int>(), Observable.Return(2));
      var observations = ObservableResources.ObserveResources(new[] { watchedResources1, watchedResources2 }).ToList().Wait();
      Assert.AreEqual(3, observations.Count);
      Assert.That(observations, Is.All.EqualTo(Enumerable.Empty<int>()));
    }

    [Test]
    public void Observing_two_watched_resources_filters_their_elements()
    {
      var watchedResources1 = new Watched<int>(new[] { 42 }, Observable.Empty<int>());
      var watchedResources2 = new Watched<int>(new[] { 9001 }, Observable.Empty<int>());
      var observable = ObservableResources.ObserveResources(new[] { watchedResources1, watchedResources2 },  i => 42 == i);
      Assert.AreEqual(new[] { new[] { 42 } },
                      observable.ToList().Wait());
    }

    [Test]
    public void Observing_two_watched_resources_filters_their_elements_and_observation_triggers()
    {
      var watchedResources1 = new Watched<int>(new[] { 42 }, Observable.Return(42));
      var watchedResources2 = new Watched<int>(new[] { 9001 }, Observable.Return(9001));
      var observable = ObservableResources.ObserveResources(new[] { watchedResources1, watchedResources2 },  i => 42 == i);
      Assert.AreEqual(new[] { new[] { 42 }, new[] { 42 } },
                      observable.ToList().Wait());
    }
  }
}