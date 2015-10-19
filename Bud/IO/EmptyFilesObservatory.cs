using System;
using System.Reactive.Linq;

namespace Bud.IO {
  public class EmptyFilesObservatory : IFilesObservatory {
    public virtual IObservable<string> CreateObserver(string sourceDir, string fileFilter, bool includeSubfolders)
      => Observable.Empty<string>();
  }
}