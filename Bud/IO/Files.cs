using System;
using System.Collections.Generic;
using System.IO;
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

    public new IObservable<Files> Watch()
      => base.Watch().Select(_ => this);

    public static Hashed<string> ToTimeHashedFile(string path)
      => new Hashed<string>(path, GetTimeHash(path));

    public static long GetTimeHash(string file)
      => File.GetLastWriteTime(file).ToFileTime();

    public static long GetTimeHash()
      => DateTime.Now.ToFileTime();

    public static bool TimeHashEquals<T>(Hashed<T> timeHashedFile, string file)
      => timeHashedFile.Hash != GetTimeHash(file);
  }
}