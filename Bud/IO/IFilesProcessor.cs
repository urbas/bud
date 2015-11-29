using System;

namespace Bud.IO {
  public interface IFilesProcessor {
    IObservable<InOut> Process(IObservable<InOut> sources);
  }
}