using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bud.V1 {
  public static class EnumerableConfApi {
    public static Conf Add<T>(this Conf conf, Key<IEnumerable<T>> dependencies, params T[] values)
      => Add(conf, dependencies, (IEnumerable<T>)values);

    public static Conf Add<T>(this Conf conf, Key<IEnumerable<T>> dependencies, IEnumerable<T> values)
      => conf.Modify(dependencies, (c, oldList) => oldList.Concat(values));

    public static Conf Add<T>(this Conf conf, Key<IImmutableList<T>> dependencies, params T[] values)
      => Add(conf, dependencies, (IEnumerable<T>)values);

    public static Conf Add<T>(this Conf conf, Key<IImmutableList<T>> dependencies, IEnumerable<T> values)
      => conf.Modify(dependencies, (c, oldList) => oldList.AddRange(values));

    public static Conf Add<T>(this Conf conf, Key<IImmutableSet<T>> dependencies, params T[] values)
      => Add(conf, dependencies, (IEnumerable<T>)values);

    public static Conf Add<T>(this Conf conf, Key<IImmutableSet<T>> dependencies, IEnumerable<T> values)
      => conf.Modify(dependencies, (c, oldList) => oldList.Union(values));
  }
}