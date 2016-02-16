using System.Collections.Generic;

namespace Bud.IO {
  public interface IValueStore<in T> {
    void Add(IEnumerable<T> newValues);
    void Remove(IEnumerable<T> oldValues);
  }
}