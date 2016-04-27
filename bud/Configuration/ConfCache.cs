using System;
using System.Collections.Immutable;
using Bud.Util;
using Bud.V1;

namespace Bud.Configuration {
  public class ConfCache {
    private ImmutableDictionary<string, object> cache = ImmutableDictionary<string, object>.Empty;
    private readonly object configValueCacheGuard = new object();

    public Option<T> TryGet<T>(Key<T> key, Func<Key<T>, Option<T>> fallbackConf) {
      CacheEntry<T> cachedValue;
      if (!TryGetFromCache(key, out cachedValue)) {
        lock (configValueCacheGuard) {
          if (!TryGetFromCache(key, out cachedValue)) {
            cachedValue = new CacheEntry<T>(key, fallbackConf);
            cache = cache.Add(key, cachedValue);
          }
        }
      }
      return cachedValue.Value;
    }

    private bool TryGetFromCache<T>(Key<T> key, out CacheEntry<T> outValue) {
      object configValue;
      if (cache.TryGetValue(key, out configValue)) {
        outValue = (CacheEntry<T>) configValue;
        return true;
      }
      outValue = null;
      return false;
    }

    private sealed class CacheEntry<T> {
      private Option<T> value;
      private bool isComputed;
      private readonly Key<T> key;
      private readonly Func<Key<T>, Option<T>> computation;

      public CacheEntry(Key<T> key, Func<Key<T>, Option<T>> computation) {
        this.key = key;
        this.computation = computation;
      }

      public Option<T> Value {
        get {
          if (isComputed) {
            return value;
          }
          lock (this) {
            if (isComputed) {
              return value;
            }
            value = computation(key);
            isComputed = true;
            return value;
          }
        }
      }
    }
  }
}