using System;
using System.Reactive.Linq;

namespace Bud.IO {
  public static class Pipes {
    public static IObservable<T> ExpandWith<T>(this IObservable<T> stream, IObservable<T> otherStream) where T : IExpandable<T>
      => stream.CombineLatest(otherStream, (collection, otherCollection) => collection.ExpandWith(otherCollection));
  }
}