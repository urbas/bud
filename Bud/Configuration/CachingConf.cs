using System;
using System.Collections.Immutable;

namespace Bud.Configuration {
  public class CachingConf {
    private ImmutableDictionary<string, object> configValueCache = ImmutableDictionary<string, object>.Empty;
    private readonly object configValueCacheGuard = new object();

    public T Get<T>(Key<T> key, Func<Key<T>, T> fallbackConf) {
      T configValue;
      return TryGetFromCache(key, out configValue) ? configValue : InvokeConfigAndCache<T>(key, fallbackConf);
    }

    private bool TryGetFromCache<T>(Key<T> key, out T outValue) {
      object configValue;
      if (configValueCache.TryGetValue(key, out configValue)) {
        outValue = (T) configValue;
        return true;
      }
      outValue = default(T);
      return false;
    }

    private T InvokeConfigAndCache<T>(string key, Func<Key<T>, T> fallbackConf) {
      lock (configValueCacheGuard) {
        T cachedValue;
        if (TryGetFromCache(key, out cachedValue)) {
          return cachedValue;
        }
        var calculatedValue = fallbackConf(key);
        configValueCache = configValueCache.Add(key, calculatedValue);
        return calculatedValue;
      }
    }
  }
}