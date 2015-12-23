using System;
using System.Collections.Generic;

namespace Bud.IO {
  public interface IFilesObservatory {
    IObservable<IEnumerable<string>> CreateObserver(string sourceDir, string fileFilter, bool includeSubfolders);
  }
}