using System;
using System.Reactive.Linq;
using NUnit.Framework;

namespace Bud.IO {
  public class WatchedTest {
    private readonly int[] resources = {42};
    private readonly IObservable<int> resourceWatcher = Observable.Return(42);

    [Test]
    public void Lister_equals_the_given_resources()
      => Assert.AreSame(resources,
                        new Watched<int>(resources, resourceWatcher).Resources);

    [Test]
    public void Watcher_equals_the_given_observable()
      => Assert.AreSame(resourceWatcher,
                        new Watched<int>(resources, resourceWatcher).Watcher);
  }
}