using System;
using System.Collections.Immutable;
using Bud.SettingsConstruction;

namespace Bud {
  public abstract class ConfigKey : Key {
    protected ConfigKey(ImmutableList<string> path, string description) : base(path, description) {}
  }

  /// <summary>
  ///   Values of this key are evaluated once only per settings compilation.
  /// </summary>
  public class ConfigKey<T> : ConfigKey {
    public new static ConfigKey<T> Define(string id, string description = null) {
      return Define(ImmutableList.Create(id), description);
    }

    public new static ConfigKey<T> Define(Key parentKey, string id, string description = null) {
      return Define(parentKey.Path.Add(id), description);
    }

    public new static ConfigKey<T> Define(Key parentKey, Key childKey) {
      return Define(parentKey.Path.AddRange(childKey.Path), childKey.Description);
    }

    public new static ConfigKey<T> Define(ImmutableList<string> path, string description = null) {
      return new ConfigKey<T>(path, description);
    }

    private ConfigKey(ImmutableList<string> path, string description) : base(path, description) {}

    public static ConfigKey<T> operator /(Key parent, ConfigKey<T> child) {
      return Define(parent, child);
    }

    public Setup Init(T configValue) {
      return settings => settings.Add(new InitializeConfig<T>(settings.Scope / this, configValue));
    }

    public Setup Init(Func<IConfig, T> configValue) {
      return settings => settings.Add(new InitializeConfig<T>(settings.Scope / this, configValue));
    }

    public Setup Init(Func<IConfig, Key, T> configValue) {
      return settings => settings.Add(new InitializeConfig<T>(settings.Scope / this, ctxt => configValue(ctxt, settings.Scope)));
    }

    public Setup Modify(T newConfigValue) {
      return settings => settings.Add(new ModifyConfig<T>(settings.Scope / this, (ctxt, oldValue) => newConfigValue));
    }

    public Setup Modify(Func<T, T> configValueMutation) {
      return settings => settings.Add(new ModifyConfig<T>(settings.Scope / this, (ctxt, oldValue) => configValueMutation(oldValue)));
    }

    public Setup Modify(Func<IConfig, T> configValueMutation) {
      return settings => settings.Add(new ModifyConfig<T>(settings.Scope / this, (ctxt, oldValue) => configValueMutation(ctxt)));
    }

    public Setup Modify(Func<IConfig, T, T> configValueMutation) {
      return settings => settings.Add(new ModifyConfig<T>(settings.Scope / this, configValueMutation));
    }

    public Setup Modify(Func<IConfig, T, Key, T> configValueMutation) {
      return settings => settings.Add(new ModifyConfig<T>(settings.Scope / this, (ctxt, oldTaskDef) => configValueMutation(ctxt, oldTaskDef, settings.Scope)));
    }
  }
}