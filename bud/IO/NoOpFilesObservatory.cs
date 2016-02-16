using System;
using System.Reactive.Linq;

namespace Bud.IO {
  public class NoOpFilesObservatory : IFilesObservatory {
    public virtual IObservable<string> CreateObserver(string dir,
                                                      string fileFilter,
                                                      bool includeSubfolders)
      => Observable.Empty<string>();
  }
}