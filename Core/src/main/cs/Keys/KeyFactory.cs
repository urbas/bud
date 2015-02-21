using System.Collections.Immutable;

namespace Bud.Keys {
  internal sealed class KeyFactory : IKeyFactory<Key> {
    public static readonly IKeyFactory<Key> Instance = new KeyFactory();

    private KeyFactory() {}

    public Key Specialize(Key key) {
      return key;
    }

    public Key Define(ImmutableList<string> path, string description) {
      return new Key(path, description);
    }
  }
}