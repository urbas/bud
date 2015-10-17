using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Configuration {
  public class CachingConf : IConf {
    private IDictionary<string, IConfigDefinition> ConfigDefinitions { get; }
    private ImmutableDictionary<string, ConfigValue> configValueCache = ImmutableDictionary<string, ConfigValue>.Empty;
    private readonly object configValueCacheGuard = new object();

    internal CachingConf(IDictionary<string, IConfigDefinition> configDefinitions) {
      ConfigDefinitions = configDefinitions;
    }

    public T Get<T>(Key<T> configKey) {
      var configValue = GetFromCacheOrInvoke(configKey.ToAbsolute());
      try {
        return (T) configValue.Value;
      } catch (Exception) {
        throw new ConfigTypeException($"Trying to read configuration '{(string)configKey}' as type '{typeof(T)}'. Its actual type is '{configValue.Value.GetType()}'.");
      }
    }

    private ConfigValue GetFromCacheOrInvoke(string configKey) {
      ConfigValue configValue;
      return configValueCache.TryGetValue(configKey, out configValue) ? configValue : InvokeConfigAndCache(configKey);
    }

    private ConfigValue InvokeConfigAndCache(string configKey) {
      lock (configValueCacheGuard) {
        ConfigValue configValue;
        if (configValueCache.TryGetValue(configKey, out configValue)) {
          return configValue;
        }
        IConfigDefinition configDefinition;
        if (ConfigDefinitions.TryGetValue(configKey, out configDefinition)) {
          configValue = new ConfigValue {ValueType = configDefinition.ValueType, Value = configDefinition.Invoke(this)};
          configValueCache = configValueCache.Add(configKey, configValue);
          return configValue;
        }
        throw new ConfigUndefinedException($"Configuration '{configKey ?? "<null>"}' is undefined.");
      }
    }

    private struct ConfigValue {
      public Type ValueType;
      public object Value;
    }
  }
}