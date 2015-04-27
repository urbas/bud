using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Logging;

namespace Bud {
  public class Config : IConfig {
    private readonly Dictionary<Key, object> ConfigEvaluationCache = new Dictionary<Key, object>();

    public Config(ImmutableDictionary<ConfigKey, IConfigDefinition> configDefinitions, ILogger logger) {
      Logger = logger;
      ConfigDefinitions = configDefinitions;
    }

    public ImmutableDictionary<ConfigKey, IConfigDefinition> ConfigDefinitions { get; }

    public ILogger Logger { get; }

    public bool IsConfigDefined(Key key) => ConfigDefinitions.ContainsKey(Key.Root / key);

    public object Evaluate(ConfigKey key) => EvaluateConfig(key);

    public T Evaluate<T>(ConfigKey<T> configKey) => (T) Evaluate((ConfigKey) configKey);

    public bool TryEvaluate<T>(ConfigKey<T> configKey, out T evaluatedValue) {
      object untypedEvaluatedValue;
      if (TryEvaluateConfig(configKey, out untypedEvaluatedValue)) {
        evaluatedValue = (T) untypedEvaluatedValue;
        return true;
      }
      evaluatedValue = default(T);
      return false;
    }

    private bool TryEvaluateConfig(Key key, out object untypedEvaluatedValue) {
      var absoluteKey = Key.Root / key;
      // TODO: verify whether we can do this optimistic fetching with dictionary. Can we read while someone is writing to the dictionary? Will we get concurrent access exceptions? Corruptions?
      if (ConfigEvaluationCache.TryGetValue(absoluteKey, out untypedEvaluatedValue)) {
        return true;
      }
      lock (ConfigEvaluationCache) {
        IConfigDefinition configDefinition;
        if (ConfigDefinitions.TryGetValue(absoluteKey, out configDefinition)) {
          untypedEvaluatedValue = configDefinition.Evaluate(new ScopedConfig(this, key));
          ConfigEvaluationCache.Add(absoluteKey, untypedEvaluatedValue);
          return true;
        }
      }
      return false;
    }

    public object EvaluateConfig(Key key) {
      object value;
      if (TryEvaluateConfig(key, out value)) {
        return value;
      }
      throw new ArgumentException(KeyUndefinedEvaluationFailedMessage(key));
    }

    public static string KeyUndefinedEvaluationFailedMessage(Key key) {
      return string.Format("Could not evaluate '{0}'. The value for this key was not defined.", key);
    }
  }
}