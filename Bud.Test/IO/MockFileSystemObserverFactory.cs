using System;
using System.IO;
using System.Reactive.Linq;

namespace Bud.IO {
  public class MockFileSystemObserverFactory : IFileSystemObserverFactory {
    public virtual IObservable<FileSystemEventArgs> Create(string sourceDir, string fileFilter, bool includeSubfolders)
      => Observable.Empty<FileSystemEventArgs>();
  }
}