using System;

namespace Bud.IO {
  public interface IFilesObservatory {
    IObservable<string> CreateObserver(string dir,
                                       string fileFilter,
                                       bool includeSubfolders);
  }
}