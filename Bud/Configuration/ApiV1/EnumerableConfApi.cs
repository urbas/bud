using System.Collections.Generic;
using System.Linq;

namespace Bud.Configuration.ApiV1 {
  public static class EnumerableConfApi {
    public static Conf Add<T>(this Conf conf, Key<IEnumerable<T>> dependencies, params T[] values)
      => Add(conf, dependencies, (IEnumerable<T>)values);

    public static Conf Add<T>(this Conf conf, Key<IEnumerable<T>> dependencies, IEnumerable<T> values)
      => conf.Modify(dependencies, (c, oldList) => oldList.Concat(values));
  }
}