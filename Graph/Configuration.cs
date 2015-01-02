using System;
using System.Collections.Generic;
using Bud.SettingsConstruction;
using System.Collections.Immutable;
using System.Text;

namespace Bud {

  public interface IConfiguration {
    ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions { get; }
    bool IsConfigDefined(Key scope);
    T Evaluate<T>(ConfigKey<T> configKey);
    object EvaluateConfig(Key scope);
  }

  // TODO: Make this class thread-safe.
  public class Configuration : IConfiguration {
    private readonly ImmutableDictionary<Key, IConfigDefinition> configDefinitions;
    private readonly Dictionary<Key, object> configValues = new Dictionary<Key, object>();

    public Configuration(ImmutableDictionary<Key, IConfigDefinition> scopeDefinitions) {
      this.configDefinitions = scopeDefinitions;
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions { get { return configDefinitions; } }

    public bool IsConfigDefined(Key scope) {
      return configDefinitions.ContainsKey(scope);
    }

    public object Evaluate(ConfigKey scope) {
      return EvaluateConfig(scope);
    }

    public T Evaluate<T>(ConfigKey<T> configKey) {
      return (T)Evaluate((ConfigKey)configKey);
    }

    public object EvaluateConfig(Key scope) {
      object value;
      if (configValues.TryGetValue(scope, out value)) {
        return value;
      }
      IConfigDefinition configDefinition;
      if (configDefinitions.TryGetValue(scope, out configDefinition)) {
        value = configDefinition.Evaluate(this);
        configValues.Add(scope, value);
        return value;
      }
      throw new ArgumentException(string.Format("Could not evaluate configuration '{0}'. The value for this configuration was not defined.", scope));
    }
  }
}

