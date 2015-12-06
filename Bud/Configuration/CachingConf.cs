using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Bud.Configuration {
  public class CachingConf {
    private ImmutableDictionary<string, object> configValueCache = ImmutableDictionary<string, object>.Empty;
    private readonly object configValueCacheGuard = new object();

    public Optional<T> TryGet<T>(Key<T> key, Func<Key<T>, Optional<T>> fallbackConf) {
      lock (configValueCacheGuard) {
        Optional<T> cachedValue;
        if (TryGetFromCache(key, out cachedValue)) {
          return cachedValue;
        }
        var calculatedValue = fallbackConf(key);
        configValueCache = configValueCache.Add(key, calculatedValue);
        return calculatedValue;
      }
    }

    private bool TryGetFromCache<T>(Key<T> key, out Optional<T> outValue) {
      object configValue;
      if (configValueCache.TryGetValue(key, out configValue)) {
        outValue = (Optional<T>) configValue;
        return true;
      }
      outValue = new Optional<T>();
      return false;
    }
  }
}