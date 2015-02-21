using System.Collections.Immutable;

namespace Bud {
  public interface IKey {
    string Id { get; }
    string Description { get; }
    ImmutableList<string> Path { get; }
    bool IsRoot { get; }
    bool IsAbsolute { get; }
    int PathDepth { get; }
    Key Parent { get; }
    bool Equals(IKey otherKey);
    bool IdsEqual(IKey otherKey);
  }
}