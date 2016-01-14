using Bud.Util;

namespace Bud.V1 {
  public interface IConf {
    Option<T> TryGet<T>(Key<T> key);
  }
}