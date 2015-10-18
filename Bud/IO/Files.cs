using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class Files : WatchedResources<Hashed<string>> {
    public static readonly Files Empty = new Files(WatchedResources<Hashed<string>>.Empty);

    public Files(Func<IEnumerable<Hashed<string>>> fileEnumerationFactory,
                 Func<IObservable<FileSystemEventArgs>> fileWatcherFactory)
      : base(fileEnumerationFactory, fileWatcherFactory) {}

    public Files(Func<IEnumerable<string>> fileEnumerationFactory,
                 Func<IObservable<FileSystemEventArgs>> fileWatcherFactory)
      : this(() => fileEnumerationFactory().Select(ToTimeHashedFile), fileWatcherFactory) {}

    public Files(IEnumerable<Hashed<string>> files)
      : this(() => files, Observable.Empty<FileSystemEventArgs>) {}

    public Files(WatchedResources<Hashed<string>> files) : base(files) {}

    public Files ExpandWith(Files other)
      => new Files(base.ExpandWith(other));

    public new Files WithFilter(Func<Hashed<string>, bool> filter)
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