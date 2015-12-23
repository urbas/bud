using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Bud.IO {
  public class EmptyFilesObservatory : IFilesObservatory {
    public virtual IObservable<IEnumerable<string>> CreateObserver(string sourceDir, string fileFilter, bool includeSubfolders)
      => Observable.Empty<IEnumerable<string>>();
  }
}