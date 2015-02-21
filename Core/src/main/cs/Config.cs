using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Logging;

namespace Bud {
  public interface IConfig {
    ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions { get; }
    ILogger Logger { get; }
    bool IsConfigDefined(Key key);
    T Evaluate<T>(ConfigKey<T> configKey);
    object EvaluateConfig(Key key);
  }

  // TODO: Make this class thread-safe.
  public class Config : IConfig {
    private readonly ImmutableDictionary<Key, IConfigDefinition> configDefinitions;
    private readonly Dictionary<Key, object> configValues = new Dictionary<Key, object>();

    public Config(ImmutableDictionary<Key, IConfigDefinition> configDefinitions, ILogger logger) {
      Logger = logger;
      this.configDefinitions = configDefinitions;
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions {
      get { return configDefinitions; }
    }

    public ILogger Logger { get; private set; }

    public bool IsConfigDefined(Key key) {
      return configDefinitions.ContainsKey(Key.Root / key);
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
      if (configValues.TryGetValue(absoluteKey, out value)) {
        return value;
      }
      IConfigDefinition configDefinition;
      if (configDefinitions.TryGetValue(absoluteKey, out configDefinition)) {
        value = configDefinition.Evaluate(this);
        configValues.Add(absoluteKey, value);
        return value;
      }
      throw new ArgumentException(string.Format("Could not evaluate configuration '{0}'. The value for this configuration was not defined.", absoluteKey));
    }
  }
}