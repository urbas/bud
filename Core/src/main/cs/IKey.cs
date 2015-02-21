using System.Collections.Immutable;

namespace Bud {
  public interface IKey {
    string Id { get; }
    string Description { get; }
    ImmutableList<string> PathComponents { get; }
    bool IsRoot { get; }
    bool IsAbsolute { get; }
    Key Parent { get; }
    Key Leaf { get; }
    string Path { get; }
    bool Equals(IKey otherKey);
  }
}