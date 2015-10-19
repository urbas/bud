using System;
using System.Linq;
using System.Reactive.Linq;
using Moq;
using NUnit.Framework;

namespace Bud.IO {
  public class WatchedResourcesTest {
    private readonly int[] resources = {42};
    private readonly int[] otherResources = {9001};

    [Test]
    public void Equals_to_the_enumeration_of_resources() {
      var watchedResources = new WatchedResources<int>(resources, Observable.Empty<int>());
      Assert.AreEqual(resources, watchedResources);
    }

    [Test]
    public void ExpandWith_produces_a_concatenated_enumeration() {
      var watchedResources = new WatchedResources<int>(resources, Observable.Empty<int>());
      var otherWatchedResources = new WatchedResources<int>(otherResources, Observable.Empty<int>());
      Assert.AreEqual(resources.Concat(otherResources),
                      watchedResources.ExpandWith(otherWatchedResources));
    }

    [Test]
    public void Filter_produces_a_filtered_watch_stream() {
      var watchedResources = new WatchedResources<int>(resources, Observable.Empty<int>());
      Assert.IsEmpty(watchedResources.WithFilter(i => 42 != i));
    }

    [Test]
    public void Watching_produces_the_first_observation() {
      var watchedResources = new WatchedResources<int>(resources, Observable.Empty<int>());
      Assert.AreEqual(new[] {resources},
                      watchedResources.Watch().ToEnumerable().ToList());
    }

    [Test]
    public void Does_not_subscribe_to_the_watcher_if_not_observing_past_first_notification() {
      var resourceWatcher = new Mock<IObservable<int>>(MockBehavior.Strict).Object;
      new WatchedResources<int>(resources, resourceWatcher).Watch().ToEnumerable().First();
    }

    [Test]
    public void Produces_watch_notifications_from_the_given_watcher() {
      var singleNotificationWatcher = Observable.Return(42);
      var watchedResources = new WatchedResources<int>(resources, singleNotificationWatcher);
      Assert.AreEqual(new[] {watchedResources, watchedResources},
                      watchedResources.Watch().ToEnumerable().ToList());
    }

    [Test]
    public void Filter_produces_a_filtered_enumeration() {
      var watchedResources = new WatchedResources<int>(resources, Observable.Return(42));
      var withFilter = watchedResources.WithFilter(i => 42 != i);
      Assert.AreEqual(new[] {withFilter},
                      withFilter.Watch().ToEnumerable().ToList());
    }

    [Test]
    public void ExpandWith_does_not_duplicate_the_first_watch_event() {
      var watchedResources = new WatchedResources<int>(resources, Observable.Empty<int>());
      var otherWatchedResources = new WatchedResources<int>(otherResources, Observable.Empty<int>());
      Assert.AreEqual(new[] {resources.Concat(otherResources)},
                      watchedResources.ExpandWith(otherWatchedResources).Watch().ToEnumerable().ToList());
    }

    [Test]
    public void ExpandWith_concatenates_notifications() {
      var singleNotificationWatcher = Observable.Return(1);
      var expandedResources = new WatchedResources<int>(resources, singleNotificationWatcher)
        .ExpandWith(new WatchedResources<int>(otherResources, singleNotificationWatcher));
      Assert.AreEqual(new[] {expandedResources, expandedResources, expandedResources},
                      expandedResources.Watch().ToEnumerable().ToList());
    }
  }
}