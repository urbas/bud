using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public static class Files {
    public static readonly IFiles Empty = new EmptyFiles();

    public static IFiles ExtendWith(this IFiles files, IFiles otherFiles)
      => new CompoundFiles(files, otherFiles);

    private class EmptyFiles : IFiles {
      public IEnumerable<string> Enumerate() => Enumerable.Empty<string>();
      public IObservable<FilesUpdate> Watch() => Observable.Return(new FilesUpdate(null, this));
    }
  }
}