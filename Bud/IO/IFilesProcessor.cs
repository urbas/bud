using System;
using System.Collections.Generic;

namespace Bud.IO {
  public interface IFilesProcessor {
    IObservable<IEnumerable<string>> Process(IObservable<IEnumerable<string>> sources);
  }
}