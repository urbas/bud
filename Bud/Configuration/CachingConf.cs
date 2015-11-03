using System.Collections.Generic;

namespace Bud.Configuration {
  public class CachingConf : IConf {
    private IDictionary<string, IConfDefinition> ConfigDefinitions { get; }
    private readonly ValueCache valueCache;

    internal CachingConf(IDictionary<string, IConfDefinition> configDefinitions) {
      ConfigDefinitions = configDefinitions;
      valueCache = new ValueCache(CalculateValue);
    }

    public T Get<T>(Key<T> key) => valueCache.Get(key.Relativize());

    private object CalculateValue(string configKey) {
      IConfDefinition confDefinition;
      if (ConfigDefinitions.TryGetValue(configKey, out confDefinition)) {
        return confDefinition.Value(this);
      }
      throw new ConfigUndefinedException($"Configuration '{configKey ?? "<null>"}' is undefined.");
    }
  }
}