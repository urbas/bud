using System;
using System.Collections.Generic;

namespace Bud.IO {
  public interface IWatchedResources<out TResource> {
    /// <summary>
    /// Returns a repeatedly enumerable collection of resources.
    /// </summary>
    IEnumerable<TResource> Lister { get; }
    /// <summary>
    /// A stream of updates to the list of resources (see <see cref="Lister"/>).
    /// </summary>
    IObservable<TResource> Watcher { get; }
  }
}