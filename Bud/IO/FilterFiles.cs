using System;
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

    public IEnumerable<string> Enumerate() => Files.Enumerate().Where(IsFileIncluded);

    public IObservable<FilesUpdate> Watch()
      => Files.Watch().Where(AnyIncludedFileChanged).Select(update => new FilesUpdate(update.FileSystemEventArgs, this));

    private bool AnyIncludedFileChanged(FilesUpdate update)
      => update.FileSystemEventArgs == null ||
         IsFileIncluded(update.FileSystemEventArgs.FullPath) ||
         WasOldPathIncluded(update);

    private bool WasOldPathIncluded(FilesUpdate update) {
      var renamedEventArgs = update.FileSystemEventArgs as RenamedEventArgs;
      return renamedEventArgs != null && IsFileIncluded(renamedEventArgs.OldFullPath);
    }
  }
}