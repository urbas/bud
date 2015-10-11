using System;

namespace Bud.Pipeline {
  public delegate IObservable<TOut> Pipe<in TIn, out TOut>(IObservable<TIn> input);
}