using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Configuration {
  public class CachingConf : IConf {
    private IDictionary<string, IConfigDefinition> ConfigDefinitions { get; }
    private ImmutableDictionary<string, object> configValueCache = ImmutableDictionary<string, object>.Empty;
    private readonly object configValueCacheGuard = new object();

    internal CachingConf(IDictionary<string, IConfigDefinition> configDefinitions) {
      ConfigDefinitions = configDefinitions;
    }

    public T Get<T>(Key<T> configKey) {
      var configValue = GetFromCacheOrInvoke(configKey.Relativize());
      try {
        return (T) configValue;
      } catch (Exception) {
        throw new ConfigTypeException($"Trying to read configuration '{(string) configKey}' as type '{typeof(T)}'. Its actual type is '{configValue.GetType()}'.");
      }
    }

    private object GetFromCacheOrInvoke(string configKey) {
      object configValue;
      return configValueCache.TryGetValue(configKey, out configValue) ? configValue : InvokeConfigAndCache(configKey);
    }

    private object InvokeConfigAndCache(string configKey) {
      lock (configValueCacheGuard) {
        object configValue;
        if (configValueCache.TryGetValue(configKey, out configValue)) {
          return configValue;
        }
        IConfigDefinition configDefinition;
        if (ConfigDefinitions.TryGetValue(configKey, out configDefinition)) {
          configValue = configDefinition.Value(this);
          configValueCache = configValueCache.Add(configKey, configValue);
          return configValue;
        }
        throw new ConfigUndefinedException($"Configuration '{configKey ?? "<null>"}' is undefined.");
      }
    }
  }
}