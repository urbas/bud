using System;
using System.Collections.Immutable;

namespace Bud.IO {
  public interface IFilesProcessor {
    IObservable<ImmutableArray<Timestamped<string>>> Process(IObservable<ImmutableArray<Timestamped<string>>> sources);
  }
}