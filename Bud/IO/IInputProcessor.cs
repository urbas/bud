using System;
using System.Collections.Generic;

namespace Bud.IO {
  public interface IInputProcessor {
    IObservable<IEnumerable<object>> Process(IObservable<IEnumerable<object>> sources);
  }
}