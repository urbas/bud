using Bud.Optional;

namespace Bud.V1 {
  public interface IConf {
    Optional<T> TryGet<T>(Key<T> key);
  }
}