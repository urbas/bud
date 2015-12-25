using System;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static System.Reactive.Linq.Observable;

namespace Bud.IO {
  public class FileWatcherTest {
    private readonly string[] files = {"A"};
    private readonly IObservable<string> changes = Return("B");

    [Test]
    public void Value_is_initialised()
      => AreSame(files, new FileWatcher(files, changes).Files);

    [Test]
    public void Changes_is_initialised()
      => AreSame(changes, new FileWatcher(files, changes).Changes);
  }
}