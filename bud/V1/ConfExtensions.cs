using System;
using System.Reactive.Linq;
using Bud.Configuration;

namespace Bud.V1 {
  public static class ConfExtensions {
    public static T Get<T>(this IConf conf, Key<T> key) {
      var val = conf.TryGet(key);
      if (val.HasValue) {
        return val.Value;
      }
      throw new ConfUndefinedException($"Configuration '{key}' is undefined.");
    }

    public static T TakeOne<T>(this IConf conf, Key<IObservable<T>> key)
      => conf.Get(key).Take(1).Wait();

    public static T Get<T>(this Conf conf, Key<T> key) {
      var val = conf.TryGet(key);
      if (val.HasValue) {
        return val.Value;
      }
      throw new ConfUndefinedException($"Configuration '{key}' is undefined.");
    }

    public static T TakeOne<T>(this Conf conf, Key<IObservable<T>> key)
      => conf.Get(key).Take(1).Wait();
  }
}