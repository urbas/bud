using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public static class Files {
    public static readonly IFiles Empty = new EmptyFiles();

    public static IFiles ExtendWith(this IFiles files, IFilesObservatory filesObservatory, string sourceDir, string fileFilter, SearchOption searchOption = SearchOption.AllDirectories)
      => files.ExtendWith(new FilesInDir(filesObservatory, sourceDir, fileFilter, searchOption));

    public static IFiles ExtendWith(this IFiles files, IFiles otherFiles)
      => new CompoundFiles(files, otherFiles);

    private class EmptyFiles : IFiles {
      public IObservable<FilesUpdate> AsObservable() => Observable.Return(new FilesUpdate(null, this));
      public IEnumerator<string> GetEnumerator() => Enumerable.Empty<string>().GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
  }
}