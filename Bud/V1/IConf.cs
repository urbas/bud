using Microsoft.CodeAnalysis;

namespace Bud.V1 {
  public interface IConf {
    Optional<T> TryGet<T>(Key<T> key);
  }
}