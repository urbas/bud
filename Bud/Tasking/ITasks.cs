namespace Bud.Tasking {
  public interface ITasks {
    T Get<T>(Key<T> taskName);
  }
}