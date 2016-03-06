using System;
using System.Collections.Immutable;
using Bud.Util;
using Bud.V1;
using static Bud.Util.Option;

namespace Bud.Configuration {
  public class CachingConf {
    private ImmutableDictionary<string, object> configValueCache = ImmutableDictionary<string, object>.Empty;
    private readonly object configValueCacheGuard = new object();

    public Option<T> TryGet<T>(Key<T> key, Func<Key<T>, Option<T>> fallbackConf) {
      Option<T> cachedValue;
      if (TryGetFromCache(key, out cachedValue)) {
        return cachedValue;
      }
      lock (configValueCacheGuard) {
        if (TryGetFromCache(key, out cachedValue)) {
          return cachedValue;
        }
        var calculatedValue = fallbackConf(key);
        configValueCache = configValueCache.Add(key, calculatedValue);
        return calculatedValue;
      }
    }

    private bool TryGetFromCache<T>(Key<T> key, out Option<T> outValue) {
      object configValue;
      if (configValueCache.TryGetValue(key, out configValue)) {
        outValue = (Option<T>) configValue;
        return true;
      }
      outValue = None<T>();
      return false;
    }
  }
}