using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public static class Files {
    public static readonly IFiles Empty = new EmptyFiles();

    public static IFiles ExtendWith(this IFiles files, IFileSystemObserverFactory fileSystemObserverFactory, string sourceDir, string fileFilter)
      => files.ExtendWith(new RecursiveDirFiles(fileSystemObserverFactory, sourceDir, fileFilter));

    public static IFiles ExtendWith(this IFiles files, IFiles otherFiles)
      => new CompoundFiles(files, otherFiles);

    private class EmptyFiles : IFiles {
      public IObservable<IFiles> AsObservable() => Observable.Return(this);
      public IEnumerator<string> GetEnumerator() => Enumerable.Empty<string>().GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
  }
}