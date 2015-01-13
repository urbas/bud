using System;
using System.Collections.Generic;
using Bud.SettingsConstruction;
using System.Collections.Immutable;
using System.Text;

namespace Bud {

  public interface IConfig {
    ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions { get; }
    bool IsConfigDefined(Key key);
    T Evaluate<T>(ConfigKey<T> configKey);
    object EvaluateConfig(Key key);
  }

  // TODO: Make this class thread-safe.
  public class Config : IConfig {
    private readonly ImmutableDictionary<Key, IConfigDefinition> configDefinitions;
    private readonly Dictionary<Key, object> configValues = new Dictionary<Key, object>();

    public Config(ImmutableDictionary<Key, IConfigDefinition> configDefinitions) {
      this.configDefinitions = configDefinitions;
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions { get { return configDefinitions; } }

    public bool IsConfigDefined(Key key) {
      return configDefinitions.ContainsKey(key);
    }

    public object Evaluate(ConfigKey key) {
      return EvaluateConfig(key);
    }

    public T Evaluate<T>(ConfigKey<T> configKey) {
      return (T)Evaluate((ConfigKey)configKey);
    }

    public object EvaluateConfig(Key key) {
      object value;
      if (configValues.TryGetValue(key, out value)) {
        return value;
      }
      IConfigDefinition configDefinition;
      if (configDefinitions.TryGetValue(key, out configDefinition)) {
        value = configDefinition.Evaluate(this);
        configValues.Add(key, value);
        return value;
      }
      throw new ArgumentException(string.Format("Could not evaluate configuration '{0}'. The value for this configuration was not defined.", key));
    }
  }
}