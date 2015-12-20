using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bud.V1 {
  public static class EnumerableConfApi {
    public static Conf Add<T>(this Conf conf, Key<IEnumerable<T>> key, params T[] values)
      => Add(conf, key, (IEnumerable<T>)values);

    public static Conf Add<T>(this Conf conf, Key<IEnumerable<T>> key, IEnumerable<T> values)
      => conf.Modify(key, (c, oldList) => oldList.Concat(values));

    public static Conf Add<T>(this Conf conf, Key<IEnumerable<T>> key, Func<IConf, T> value)
      => conf.Modify(key, (c, oldList) => oldList.Concat(new [] {value(c)}));

    public static Conf Add<T>(this Conf conf, Key<IImmutableList<T>> key, params T[] values)
      => Add(conf, key, (IEnumerable<T>)values);

    public static Conf Add<T>(this Conf conf, Key<IImmutableList<T>> key, IEnumerable<T> values)
      => conf.Modify(key, (c, oldList) => oldList.AddRange(values));

    public static Conf Add<T>(this Conf conf, Key<IImmutableList<T>> key, Func<IConf, T> value)
      => conf.Modify(key, (c, oldList) => oldList.Add(value(c)));

    public static Conf Add<T>(this Conf conf, Key<IImmutableSet<T>> key, params T[] values)
      => Add(conf, key, (IEnumerable<T>)values);

    public static Conf Add<T>(this Conf conf, Key<IImmutableSet<T>> key, IEnumerable<T> values)
      => conf.Modify(key, (c, oldList) => oldList.Union(values));

    public static Conf Add<T>(this Conf conf, Key<IImmutableSet<T>> key, Func<IConf, T> value)
      => conf.Modify(key, (c, oldList) => oldList.Add(value(c)));
  }
}