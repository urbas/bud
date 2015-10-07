using System;
using System.IO;
using System.Reactive.Linq;

namespace Bud.IO {
  public class EmptyFilesObservatory : IFilesObservatory {
    public virtual IObservable<FileSystemEventArgs> CreateObserver(string sourceDir, string fileFilter, bool includeSubfolders)
      => Observable.Empty<FileSystemEventArgs>();
  }
}