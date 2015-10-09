using System;
using System.Collections.Generic;

namespace Bud.IO {
  public interface IFiles {
    IEnumerable<string> Enumerate();
    IObservable<FilesUpdate> Watch();
  }
}