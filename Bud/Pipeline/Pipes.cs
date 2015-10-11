using System;

namespace Bud.Pipeline {
  public static class Pipes {
    public static IObservable<TOut> PipeInto<TIn, TOut>(this IObservable<TIn> inputStream, Pipe<TIn, TOut> pipe)
      => pipe(inputStream);
  }
}