using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.V1 {
  public static class EnumerableConfApi {
    #region IEnumerable

    public static Conf Clear<T>(this Conf conf, Key<IEnumerable<T>> key)
      => conf.SetValue(key, Enumerable.Empty<T>());

    public static Conf Add<T>(this Conf conf, Key<IEnumerable<T>> key, params T[] values)
      => Add(conf, key, (IEnumerable<T>) values);

    public static Conf Add<T>(this Conf conf, Key<IEnumerable<T>> key, IEnumerable<T> values)
      => conf.Modify(key, (c, oldList) => oldList.Concat(values));

    public static Conf Add<T>(this Conf conf, Key<IEnumerable<T>> key, Func<IConf, T> value)
      => conf.Modify(key, (c, oldList) => oldList.Concat(new[] {value(c)}));

    public static Conf Add<T>(this Conf conf, Key<IObservable<IEnumerable<T>>> key, T value)
      => conf.Add(key, _ => Observable.Return(value));

    public static Conf Add<T>(this Conf conf, Key<IObservable<IEnumerable<T>>> key, IObservable<T> value)
      => conf.Add(key, _ => value);

    public static Conf Add<T>(this Conf conf, Key<IObservable<IEnumerable<T>>> key, Func<IConf, IObservable<T>> value)
      => conf.Add(key, c => value(c).Select(e => new[] {e}));

    public static Conf Add<T>(this Conf conf, Key<IObservable<IEnumerable<T>>> key, Func<IConf, IObservable<IEnumerable<T>>> values)
      => conf.Modify(key, (c, observable) => observable.CombineLatest(values(c), Enumerable.Concat));

    #endregion

    #region IImmutableList

    public static Conf Clear<T>(this Conf conf, Key<IImmutableList<T>> key)
      => conf.SetValue(key, ImmutableList<T>.Empty);

    public static Conf Add<T>(this Conf conf, Key<IImmutableList<T>> key, params T[] values)
      => Add(conf, key, (IEnumerable<T>) values);

    public static Conf Add<T>(this Conf conf, Key<IImmutableList<T>> key, IEnumerable<T> values)
      => conf.Modify(key, (c, oldList) => oldList.AddRange(values));

    public static Conf Add<T>(this Conf conf, Key<IImmutableList<T>> key, Func<IConf, T> value)
      => conf.Modify(key, (c, oldList) => oldList.Add(value(c)));

    public static Conf Add<T>(this Conf conf, Key<IObservable<IImmutableList<T>>> key, T value)
      => conf.Add(key, _ => Observable.Return(value));

    public static Conf Add<T>(this Conf conf, Key<IObservable<IImmutableList<T>>> key, IObservable<T> value)
      => conf.Add(key, _ => value);

    public static Conf Add<T>(this Conf conf, Key<IObservable<IImmutableList<T>>> key, Func<IConf, IObservable<T>> value)
      => conf.Add(key, c => value(c).Select(ImmutableList.Create));

    public static Conf Add<T>(this Conf conf, Key<IObservable<IImmutableList<T>>> key, Func<IConf, IObservable<IEnumerable<T>>> values)
      => conf.Modify(key, (c, observable) => observable
                            .CombineLatest(values(c),
                                           (list, listToAppend) => list.AddRange(listToAppend)));

    #endregion

    #region IImmutableSet

    public static Conf Clear<T>(this Conf conf, Key<IImmutableSet<T>> key)
      => conf.SetValue(key, ImmutableHashSet<T>.Empty);

    public static Conf Add<T>(this Conf conf, Key<IImmutableSet<T>> key, params T[] values)
      => Add(conf, key, (IEnumerable<T>) values);

    public static Conf Add<T>(this Conf conf, Key<IImmutableSet<T>> key, IEnumerable<T> values)
      => conf.Modify(key, (c, oldList) => oldList.Union(values));

    public static Conf Add<T>(this Conf conf, Key<IImmutableSet<T>> key, Func<IConf, T> value)
      => conf.Modify(key, (c, oldList) => oldList.Add(value(c)));

    public static Conf Add<T>(this Conf conf, Key<IObservable<IImmutableSet<T>>> key, T value)
      => conf.Add(key, _ => Observable.Return(value));

    public static Conf Add<T>(this Conf conf, Key<IObservable<IImmutableSet<T>>> key, IObservable<T> value)
      => conf.Add(key, _ => value);

    public static Conf Add<T>(this Conf conf, Key<IObservable<IImmutableSet<T>>> key, Func<IConf, IObservable<T>> value)
      => conf.Add(key, c => value(c).Select(v => new [] {v}));

    public static Conf Add<T>(this Conf conf, Key<IObservable<IImmutableSet<T>>> key, Func<IConf, IObservable<IEnumerable<T>>> values)
      => conf.Modify(key, (c, observable) => observable
                            .CombineLatest(values(c),
                                           (list, listToAppend) => list.Union(listToAppend)));

    #endregion
  }
}