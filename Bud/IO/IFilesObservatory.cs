using System;

namespace Bud.IO {
  public interface IFilesObservatory {
    IObservable<string> CreateObserver(string sourceDir, string fileFilter, bool includeSubfolders);
  }
}