using System.Collections.Immutable;

namespace Bud.Keys {
  internal interface IKeyFactory<out T> {
    T Define(ImmutableList<string> path, string description);
  }
}