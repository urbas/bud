using System;

namespace Bud.IO {
  public class LocalFilesObservatory : IFilesObservatory {
    public IObservable<string> CreateObserver(string dir,
                                              string fileFilter,
                                              bool includeSubfolders)
      => FilesObservatory.ObserveFileSystem(dir, fileFilter, includeSubfolders);
  }
}