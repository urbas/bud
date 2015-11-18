using System.Collections.Immutable;

namespace Bud.Configuration {
  public class ConfValueCache {
    private readonly IConf wrappedConf;
    private ImmutableDictionary<string, object> configValueCache = ImmutableDictionary<string, object>.Empty;
    private readonly object configValueCacheGuard = new object();

    public ConfValueCache(IConf wrappedConf) {
      this.wrappedConf = wrappedConf;
    }

    public T Get<T>(Key<T> key) {
      T configValue;
      return TryGetFromCache(key, out configValue) ? configValue : InvokeConfigAndCache<T>(key);
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

    private T InvokeConfigAndCache<T>(string key) {
      lock (configValueCacheGuard) {
        T cachedValue;
        if (TryGetFromCache(key, out cachedValue)) {
          return cachedValue;
        }
        var calculatedValue = wrappedConf.Get<T>(key);
        configValueCache = configValueCache.Add(key, calculatedValue);
        return calculatedValue;
      }
    }
  }
}