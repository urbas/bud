using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class FilterFiles : IFiles {
    public IFiles Files { get; }
    public Func<string, bool> IsFileIncluded { get; }

    public FilterFiles(IFiles files, Func<string, bool> isFileIncluded) {
      Files = files;
      IsFileIncluded = isFileIncluded;
    }

    public IEnumerator<string> GetEnumerator() => FilteredFiles.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) FilteredFiles).GetEnumerator();
    public IEnumerable<string> FilteredFiles => Files.Where(IsFileIncluded);
    public IObservable<FilesUpdate> AsObservable() => Files.AsObservable().Where(AnyIncludedFileChanged);

    private bool AnyIncludedFileChanged(FilesUpdate update)
      => update.FileSystemEventArgs == null ||
         IsFileIncluded(update.FileSystemEventArgs.FullPath) ||
         IsOldPathIncluded(update);

    private bool IsOldPathIncluded(FilesUpdate update) {
      var renamedEventArgs = update.FileSystemEventArgs as RenamedEventArgs;
      return renamedEventArgs != null && IsFileIncluded(renamedEventArgs.OldFullPath);
    }
  }
}