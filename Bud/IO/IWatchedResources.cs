using System;
using System.Collections.Generic;

namespace Bud.IO {
  public interface IWatchedResources<out TResource> : IEnumerable<TResource> {
    IEnumerable<TResource> Resources { get; }
    IObservable<TResource> Watcher { get; }
    IObservable<IWatchedResources<TResource>> Watch();
  }
}