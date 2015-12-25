using System.Linq;
using NUnit.Framework;
using static System.Reactive.Linq.Observable;
using static NUnit.Framework.Assert;

namespace Bud.IO {
  public class FileWatchersTest {
    [Test]
    public void ToObservable_produces_the_first_observation()
      => That(new FileWatcher("a").ToObservable().ToList().Wait(),
        Has.Exactly(1).EqualTo(new[] { "a" }));

    [Test]
    public void ToObservable_produces_more_observations()
      => That(new FileWatcher(new[] {"a"}, Return("a")).ToObservable().ToList().Wait(),
          Has.Exactly(2).EqualTo(new[] { "a" }));

    [Test]
    public void ToObservable_filters_values_and_changes()
      => That(WatchedArrayWithTwoChanges().ToObservable(s => s[0] >= 'a').ToList().Wait(),
              Has.Exactly(2).EqualTo(new[] {"b", "d"}));

    [Test]
    public void ToObservable_from_many_watchers_produces_an_empty_observation()
      => That(Enumerable.Empty<FileWatcher>().ToObservable(_ => true).ToList().Wait(),
              Has.Exactly(1).Empty);

    [Test]
    public void ToObservable_from_many_watchers_produces_an_some_observations()
      => That(new[] {WatchedArrayWithTwoChanges(), WatchedArrayWithOneChange()}
                .ToObservable(_ => true).ToList().Wait(),
              Has.Exactly(4).EqualTo(new[] {"A", "b", "C", "d", "E", "f"}));

    [Test]
    public void ToObservable_from_many_watchers_filters_observations()
      => That(new[] {WatchedArrayWithTwoChanges(), WatchedArrayWithOneChange()}
                .ToObservable(s => s[0] >= 'a').ToList().Wait(),
              Has.Exactly(3).EqualTo(new[] {"b", "d", "f",}));

    private static FileWatcher WatchedArrayWithTwoChanges()
      => new FileWatcher(new[] {"A", "b", "C", "d"},
                         Return("a").Concat(Return("B")));

    private static FileWatcher WatchedArrayWithOneChange()
      => new FileWatcher(new[] {"E", "f"}, Return("a"));
  }
}