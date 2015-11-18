using System.Collections.Generic;

namespace Bud.Configuration {
  public class ConfValueCalculator {
    public IDictionary<string, IConfDefinition> ConfDefinitions { get; }

    public ConfValueCalculator(IDictionary<string, IConfDefinition> confDefinitions) {
      ConfDefinitions = confDefinitions;
    }

    public T Get<T>(Key<T> key, IConf conf) {
      IConfDefinition confDefinition;
      if (ConfDefinitions.TryGetValue(key, out confDefinition)) {
        return (T) confDefinition.Value(conf);
      }
      throw new ConfUndefinedException($"Configuration '{key}' is undefined.");
    }
  }
}