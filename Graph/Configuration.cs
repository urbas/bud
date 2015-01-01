using System;
using System.Collections.Generic;
using Bud.SettingsConstruction;
using System.Collections.Immutable;
using System.Text;

namespace Bud {

  public interface IConfiguration {
    ImmutableDictionary<Scope, IConfigDefinition> ConfigDefinitions { get; }
    bool IsConfigDefined(Scope scope);
    T Evaluate<T>(ConfigKey<T> configKey);
    object EvaluateConfig(Scope scope);
  }

  // TODO: Make this class thread-safe.
  public class Configuration : IConfiguration {
    private readonly ImmutableDictionary<Scope, IConfigDefinition> configDefinitions;
    private readonly Dictionary<Scope, object> configValues = new Dictionary<Scope, object>();

    public Configuration(ImmutableDictionary<Scope, IConfigDefinition> scopeDefinitions) {
      this.configDefinitions = scopeDefinitions;
    }

    public ImmutableDictionary<Scope, IConfigDefinition> ConfigDefinitions { get { return configDefinitions; } }

    public bool IsConfigDefined(Scope scope) {
      return configDefinitions.ContainsKey(scope);
    }

    public object Evaluate(ConfigKey scope) {
      return EvaluateConfig(scope);
    }

    public T Evaluate<T>(ConfigKey<T> configKey) {
      return (T)Evaluate((ConfigKey)configKey);
    }

    public object EvaluateConfig(Scope scope) {
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

