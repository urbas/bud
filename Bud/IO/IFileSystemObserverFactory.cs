using System;
using System.IO;

namespace Bud.IO {
  public interface IFileSystemObserverFactory {
    IObservable<FileSystemEventArgs> Create(string sourceDir, string fileFilter, bool includeSubfolders);
  }
}