using System;
using System.Reactive.Linq;
using NUnit.Framework;

namespace Bud.IO {
  public class WatchedTest {
    private readonly object value = new object();
    private readonly IObservable<object> resourceChanges = Observable.Return(new object());

    [Test]
    public void Resource_equals_the_given_resource()
      => Assert.AreSame(value,
                        Watched.Watch(value, resourceChanges).Value);

    [Test]
    public void Watcher_equals_the_given_observable()
      => Assert.AreSame(resourceChanges,
                        Watched.Watch(value, resourceChanges).Changes);
  }
}