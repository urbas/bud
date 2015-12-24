using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static Bud.IO.Watcher;
using static System.Reactive.Linq.Observable;

namespace Bud.IO {
  public class WatchedTest {
    private readonly object value = new object();
    private readonly IObservable<object> changes = Return(new object());

    [Test]
    public void Value_is_initialised()
      => AreSame(value, Watch(value, changes).Value);

    [Test]
    public void Changes_is_initialised()
      => AreSame(changes, Watch(value, changes).Changes);


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
    public void Observing_zero_watched_resources_triggers_a_single_observation()
      => AreEqual(new[] {new int[] {}},
                  ObserveResources<int>().ToList().Wait());

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
  }
}