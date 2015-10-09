using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public static class Files {
    public static readonly IFiles Empty = EmptyFiles.Instance;

    public static IFiles ExtendWith(this IFiles files, IFiles otherFiles)
      => new CompoundFiles(files, otherFiles);

    private class EmptyFiles : IFiles {
      public static readonly EmptyFiles Instance = new EmptyFiles();
      private static readonly IObservable<FilesUpdate> EmptyWatcher = Observable.Return(new FilesUpdate(null, Instance));
      public IEnumerable<string> Enumerate() => Enumerable.Empty<string>();
      public IObservable<FilesUpdate> Watch() => EmptyWatcher;
    }
  }
}