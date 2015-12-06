using Microsoft.CodeAnalysis;

namespace Bud {
  public interface IConf {
    Optional<T> TryGet<T>(Key<T> key);
  }
}