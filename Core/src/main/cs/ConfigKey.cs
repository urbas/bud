using System;
using System.Collections.Immutable;
using Bud.Plugins.Deps;
using Bud.SettingsConstruction;

namespace Bud {

  public abstract class ConfigKey : Key {
    protected ConfigKey(string id) : base(id) {}
    protected ConfigKey(string id, Key parent) : base(id, parent) {}
  }

  /// <summary>
  /// Values of this key are evaluated once only per settings compilation.
  /// </summary>
  public class ConfigKey<T> : ConfigKey {
    public ConfigKey(string id) : base(id) {}

    private ConfigKey(string id, Key parent) : base(id, parent) {}

    public new ConfigKey<T> In(Key parent) {
      if (parent.IsGlobal) {
        return this;
      }
      return new ConfigKey<T>(Id, Concat(parent, Parent));
    }

    public Func<Settings, Settings> Init(T configValue) {
      return settings => settings.Add(new InitializeConfig<T>(In(settings.Scope), configValue));
    }

    public Func<Settings, Settings> Init(Func<IConfig, T> configValue) {
      return settings => settings.Add(new InitializeConfig<T>(In(settings.Scope), configValue));
    }

    public Func<Settings, Settings> Init(Func<IConfig, Key, T> configValue) {
      return settings => settings.Add(new InitializeConfig<T>(In(settings.Scope), ctxt => configValue(ctxt, settings.Scope)));
    }

    public Func<Settings, Settings> Modify(T newConfigValue) {
      return settings => settings.Add(new ModifyConfig<T>(In(settings.Scope), (ctxt, oldValue) => newConfigValue));
    }

    public Func<Settings, Settings> Modify(Func<T, T> configValueMutation) {
      return settings => settings.Add(new ModifyConfig<T>(In(settings.Scope), (ctxt, oldValue) => configValueMutation(oldValue)));
    }

    public Func<Settings, Settings> Modify(Func<IConfig, T> configValueMutation) {
      return settings => settings.Add(new ModifyConfig<T>(In(settings.Scope), (ctxt, oldValue) => configValueMutation(ctxt)));
    }

    public Func<Settings, Settings> Modify(Func<IConfig, T, T> configValueMutation) {
      return settings => settings.Add(new ModifyConfig<T>(In(settings.Scope), configValueMutation));
    }
  }
}

