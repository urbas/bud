using System;
using System.Collections.Generic;
using Bud.SettingsConstruction;

namespace Bud {
  // TODO: Make this class thread-safe.
  public class Configuration {
    public readonly ScopeDefinitions ScopeDefinitions;
    private readonly Dictionary<Scope, object> configValues = new Dictionary<Scope, object>();

    protected Configuration(ScopeDefinitions scopeDefinitions) {
      this.ScopeDefinitions = scopeDefinitions;
    }

    public bool IsScopeDefined(Scope scope) {
      return IsConfigDefined(scope) || IsTaskDefined(scope);
    }

    public bool IsConfigDefined(Scope scope) {
      return ScopeDefinitions.ConfigDefinitions.ContainsKey(scope);
    }

    public bool IsTaskDefined(Scope scope) {
      return ScopeDefinitions.TaskDefinitions.ContainsKey(scope);
    }

    public object Evaluate(ConfigKey scope) {
      return EvaluateConfig(scope);
    }

    public T Evaluate<T>(ConfigKey<T> configKey) {
      return (T)Evaluate((ConfigKey)configKey);
    }

    protected object EvaluateConfig(Scope scope) {
      object value;
      if (configValues.TryGetValue(scope, out value)) {
        return value;
      }
      IConfigDefinition configDefinition;
      if (ScopeDefinitions.ConfigDefinitions.TryGetValue(scope, out configDefinition)) {
        value = configDefinition.Evaluate(this);
        configValues.Add(scope, value);
        return value;
      }
      throw new ArgumentException(string.Format("Could not evaluate configuration '{0}'. The value for this configuration was not defined.", scope));
    }
  }
}

