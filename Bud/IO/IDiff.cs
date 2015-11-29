using System.Collections.Immutable;

namespace Bud.IO {
  public interface IDiff<T> {
    ImmutableHashSet<T> Added { get; }
    ImmutableHashSet<T> Removed { get; }
    ImmutableHashSet<T> Changed { get; }
    ImmutableHashSet<T> All { get; }
  }
}