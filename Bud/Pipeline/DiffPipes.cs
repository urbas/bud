using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;

namespace Bud.Pipeline {
  public static class DiffPipes {
    public static IObservable<Diff<T>> ToDiffPipe<T>(this IObservable<IEnumerable<Timestamped<T>>> pipe) {
      var previousDiff = Diff.Empty<T>();
      return pipe.Select(enumerable => previousDiff = previousDiff.NextDiff(enumerable));
    }
  }
}