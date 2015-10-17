using System;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public static class FilesStream {
    public static IObservable<Files> AddFiles(this IObservable<Files> stream, IObservable<Files> otherStream)
      => stream.CombineLatest(otherStream, (collection, otherCollection) => new Files(Enumerable.Concat(collection, otherCollection)));
  }
}