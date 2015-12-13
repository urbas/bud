using System;

namespace Bud.IO {
  public interface IInputProcessor {
    IObservable<InOut> Process(IObservable<InOut> sources);
  }
}