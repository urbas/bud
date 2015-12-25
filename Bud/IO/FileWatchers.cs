using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public static class FileWatchers {
    public static IObservable<IEnumerable<string>>
      ToObservable(this FileWatcher watcher)
      => ToObservable(watcher.Files, watcher.Changes);

    public static IObservable<IEnumerable<string>> ToObservable(this FileWatcher watcher,
                                                                Func<string, bool> filter)
      => ToObservable(watcher.Files, watcher.Changes, filter);

    public static IObservable<IEnumerable<string>>
      ToObservable(this IEnumerable<FileWatcher> watchers,
                   Func<string, bool> filter)
      => ToObservable(watchers.SelectMany(watcher => watcher.Files),
                      watchers.Select(resources => resources.Changes).Merge(),
                      filter);

    private static IObservable<IEnumerable<string>>
      ToObservable(IEnumerable<string> values,
                   IObservable<string> changes,
                   Func<string, bool> filter)
      => ToObservable(values.Where(filter), changes.Where(filter));

    private static IObservable<IEnumerable<string>>
      ToObservable(IEnumerable<string> value,
                   IObservable<string> changes)
      => Observable.Return(value).Concat(changes.Select(_ => value));
  }
}