namespace Bud {
  public interface IConfigs {
    T Get<T>(Key<T> configKey);
  }
}