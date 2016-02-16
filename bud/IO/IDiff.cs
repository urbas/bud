using System.Collections.Immutable;

namespace Bud.IO {
  public interface IDiff<T> {
    IImmutableSet<T> Added { get; }
    IImmutableSet<T> Removed { get; }
    IImmutableSet<T> Changed { get; }
    IImmutableSet<T> All { get; }
  }
}