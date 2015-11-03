using System;
using System.Collections.Immutable;

namespace Bud.Configuration {
  public class ValueCache {
    private readonly Func<string, object> valueCalculator;
    private ImmutableDictionary<string, object> configValueCache = ImmutableDictionary<string, object>.Empty;
    private readonly object configValueCacheGuard = new object();

    public ValueCache(Func<string, object> valueCalculator) {
      this.valueCalculator = valueCalculator;
    }

    public T Get<T>(Key<T> key) {
      var configValue = GetFromCacheOrInvoke(key);
      try {
        return (T) configValue;
      } catch (Exception) {
        throw new ConfigTypeException($"Trying to read configuration '{(string) key}' as type '{typeof(T)}'. Its actual type is '{configValue.GetType()}'.");
      }
    }

    private object GetFromCacheOrInvoke(string configKey) {
      object configValue;
      return configValueCache.TryGetValue(configKey, out configValue) ? configValue : InvokeConfigAndCache(configKey);
    }

    private object InvokeConfigAndCache(string key) {
      lock (configValueCacheGuard) {
        object configValue;
        if (configValueCache.TryGetValue(key, out configValue)) {
          return configValue;
        }
        configValue = valueCalculator(key);
        configValueCache = configValueCache.Add(key, configValue);
        return configValue;
      }
    }
  }
}