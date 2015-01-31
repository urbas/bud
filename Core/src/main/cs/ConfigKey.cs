using System;
using Bud.SettingsConstruction;

namespace Bud {
  public abstract class ConfigKey : Key {
    protected ConfigKey(string id, string description = null) : base(id, description) {}
    protected ConfigKey(string id, Key parent, string description = null) : base(id, parent, description) {}
  }

  /// <summary>
  ///   Values of this key are evaluated once only per settings compilation.
  /// </summary>
  public class ConfigKey<T> : ConfigKey {
    public ConfigKey(string id, string description = null) : base(id, description) {}

    public ConfigKey(string id, Key parent, string description = null) : base(id, parent, description) {}

    public new ConfigKey<T> In(Key parent) {
      return new ConfigKey<T>(Id, Concat(parent, Parent), Description);
    }

    public static ConfigKey<T> operator /(Key parent, ConfigKey<T> child) {
      return child.In(parent);
    }

    public Setup Init(T configValue) {
      return settings => settings.Add(new InitializeConfig<T>(In(settings.Scope), configValue));
    }

    public Setup Init(Func<IConfig, T> configValue) {
      return settings => settings.Add(new InitializeConfig<T>(In(settings.Scope), configValue));
    }

    public Setup Init(Func<IConfig, Key, T> configValue) {
      return settings => settings.Add(new InitializeConfig<T>(In(settings.Scope), ctxt => configValue(ctxt, settings.Scope)));
    }

    public Setup Modify(T newConfigValue) {
      return settings => settings.Add(new ModifyConfig<T>(In(settings.Scope), (ctxt, oldValue) => newConfigValue));
    }

    public Setup Modify(Func<T, T> configValueMutation) {
      return settings => settings.Add(new ModifyConfig<T>(In(settings.Scope), (ctxt, oldValue) => configValueMutation(oldValue)));
    }

    public Setup Modify(Func<IConfig, T> configValueMutation) {
      return settings => settings.Add(new ModifyConfig<T>(In(settings.Scope), (ctxt, oldValue) => configValueMutation(ctxt)));
    }

    public Setup Modify(Func<IConfig, T, T> configValueMutation) {
      return settings => settings.Add(new ModifyConfig<T>(In(settings.Scope), configValueMutation));
    }

    public Setup Modify(Func<IConfig, T, Key, T> configValueMutation) {
      return settings => settings.Add(new ModifyConfig<T>(In(settings.Scope), (ctxt, oldTaskDef) => configValueMutation(ctxt, oldTaskDef, settings.Scope)));
    }
  }
}