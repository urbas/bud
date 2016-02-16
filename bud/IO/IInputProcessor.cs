using System;
using System.Collections.Generic;

namespace Bud.IO {
  public interface IInputProcessor {
    IObservable<IEnumerable<string>> Process(IObservable<IEnumerable<string>> sources);
  }
}