using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Logging;

namespace Bud {
  public class Config : IConfig {
    private readonly Dictionary<Key, object> ConfigEvaluationCache = new Dictionary<Key, object>();

    public Config(ImmutableDictionary<Key, IConfigDefinition> configDefinitions, ILogger logger) {
      Logger = logger;
      ConfigDefinitions = configDefinitions;
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions { get; }

    public ILogger Logger { get; }

    public bool IsConfigDefined(Key key) {
      return ConfigDefinitions.ContainsKey(Key.Root / key);
    }

    public object Evaluate(ConfigKey key) {
      return EvaluateConfig(key);
    }

    public T Evaluate<T>(ConfigKey<T> configKey) {
      return (T) Evaluate((ConfigKey) configKey);
    }

    public object EvaluateConfig(Key key) {
      object value;
      var absoluteKey = Key.Root / key;
      // TODO: verify whether we can do this optimistic fetching with dictionary. Can we read while someone is writing to the dictionary? Will we get concurrent access exceptions? Corruptions?
      if (ConfigEvaluationCache.TryGetValue(absoluteKey, out value))
      {
        return value;
      }
      IConfigDefinition configDefinition;
      lock (ConfigEvaluationCache) {
        if (ConfigDefinitions.TryGetValue(absoluteKey, out configDefinition)) {
          value = configDefinition.Evaluate(new ScopedConfig(this, key));
          ConfigEvaluationCache.Add(absoluteKey, value);
          return value;
        }
      }
      throw new ArgumentException(string.Format("Could not evaluate configuration '{0}'. The value for this configuration was not defined.", absoluteKey));
    }
  }
}