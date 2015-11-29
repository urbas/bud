using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class Files : WatchedResources<string> {
    public new static readonly Files Empty = new Files(WatchedResources<string>.Empty);

    public Files(IEnumerable<string> fileEnumerationFactory,
                 IObservable<string> fileWatcher)
      : base(fileEnumerationFactory, fileWatcher) {}

    public Files(IEnumerable<string> files)
      : this(files, Observable.Empty<string>()) {}

    public Files(IWatchedResources<string> files) : base(files) {}

    public Files ExpandWith(Files other)
      => new Files(base.ExpandWith(other));

    public new Files WithFilter(Func<string, bool> filter)
      => new Files(base.WithFilter(filter));

    public static Timestamped<string> ToTimestampedFile(string path)
      => new Timestamped<string>(path, GetFileTimestamp(path));

    public static long GetFileTimestamp(string path)
      => File.GetLastWriteTime(path).ToFileTime();

    public static long FileTimestampNow()
      => DateTime.Now.ToFileTime();

    public static IEnumerable<Timestamped<string>> ToTimestampedFiles(IEnumerable<string> sources)
      => sources.Select(ToTimestampedFile);
  }
}