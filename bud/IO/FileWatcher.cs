using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;

namespace Bud.IO {
  public class FileWatcher {
    /// <summary>
    ///   This enumeration can be enumerated multiple times.
    /// </summary>
    public IEnumerable<string> Files { get; }

    /// <summary>
    ///   Notifications of changed files.
    /// </summary>
    public IObservable<string> Changes { get; }

    public FileWatcher(IEnumerable<string> files,
                       IObservable<string> changes) {
      Files = files;
      Changes = changes;
    }

    public FileWatcher(string file)
      : this(ImmutableList.Create(file), Observable.Empty<string>()) {}
  }
}