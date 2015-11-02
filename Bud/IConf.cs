namespace Bud {
  public interface IConf {
    T Get<T>(Key<T> key);
  }
}