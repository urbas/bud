namespace Bud.IO {
  public interface IExpandable<T> {
    T ExpandWith(T other);
  }
}