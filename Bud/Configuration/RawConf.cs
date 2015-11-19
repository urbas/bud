using System.Collections.Generic;

namespace Bud.Configuration {
  public class RawConf : IConf {
    public IDictionary<string, IConfDefinition> ConfDefinitions { get; }
    private CachingConf CachingConf { get; }

    public RawConf(IDictionary<string, IConfDefinition> confDefinitions) {
      ConfDefinitions = confDefinitions;
      CachingConf = new CachingConf();
    }

    public T Get<T>(Key<T> key) => CachingConf.Get(key.Relativize(), RawGet);

    public T RawGet<T>(Key<T> key) {
      IConfDefinition confDefinition;
      if (ConfDefinitions.TryGetValue(key, out confDefinition)) {
        return (T) confDefinition.Value(this);
      }
      throw new ConfUndefinedException($"Configuration '{key}' is undefined.");
    }
  }
}