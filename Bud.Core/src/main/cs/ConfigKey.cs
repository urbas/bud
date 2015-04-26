using System;
using System.Collections.Immutable;
using Bud.SettingsConstruction;

namespace Bud {
  public class ConfigKey : IKey {
    protected internal readonly Key UnderlyingKey;

    protected ConfigKey(Key underlyingKey) {
      UnderlyingKey = underlyingKey;
    }

    public static implicit operator ConfigKey(Key key) => new ConfigKey(key);

    public static implicit operator Key(ConfigKey key) => key.UnderlyingKey;

    public static Key operator /(ConfigKey parent, Key child) => Key.Define(parent, child);

    public static Key operator /(ConfigKey parent, string id) => Key.Define(parent, id);

    public string Id => UnderlyingKey.Id;

    public string Description => UnderlyingKey.Description;

    public ImmutableList<string> PathComponents => UnderlyingKey.PathComponents;

    public bool IsRoot => UnderlyingKey.IsRoot;

    public bool IsAbsolute => UnderlyingKey.IsAbsolute;

    public Key Parent => UnderlyingKey.Parent;

    public Key Leaf => UnderlyingKey.Leaf;

    public string Path => UnderlyingKey.Path;

    public bool Equals(IKey otherKey) => UnderlyingKey.Equals(otherKey);

    public override int GetHashCode() => UnderlyingKey.GetHashCode();

    public override bool Equals(object obj) => obj != null && obj.Equals(UnderlyingKey);

    public override string ToString() => UnderlyingKey.ToString();
  }

  public class ConfigKey<T> : ConfigKey {
    private ConfigKey(Key underlyingKey) : base(underlyingKey) {}

    public static implicit operator ConfigKey<T>(Key key) => new ConfigKey<T>(key);

    public static implicit operator Key(ConfigKey<T> key) => key.UnderlyingKey;

    public static ConfigKey<T> operator /(Key parent, ConfigKey<T> child) => Key.Define(parent, child);

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