using System;
using System.IO;

namespace Bud.IO {
  public interface IFilesObservatory {
    IObservable<FileSystemEventArgs> CreateObserver(string sourceDir, string fileFilter, bool includeSubfolders);
  }
}