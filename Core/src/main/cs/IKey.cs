using System.Collections.Immutable;

namespace Bud {
  public interface IKey {
    string Id { get; }
    string Description { get; }
    ImmutableList<string> PathComponents { get; }
    bool IsRoot { get; }
    bool IsAbsolute { get; }
    Key Parent { get; }
    string Path { get; }
    bool Equals(IKey otherKey);
    // TODO: Remove this one. Replace it with 'Leaf'.
    bool IdsEqual(IKey otherKey);
  }
}