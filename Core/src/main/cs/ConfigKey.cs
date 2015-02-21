using System;
using System.Collections.Immutable;
using Bud.SettingsConstruction;

namespace Bud {
  public class ConfigKey : IKey {
    protected internal readonly Key UnderlyingKey;

    protected ConfigKey(Key underlyingKey) {
      UnderlyingKey = underlyingKey;
    }

    public static implicit operator ConfigKey(Key key) {
      return new ConfigKey(key);
    }

    public static implicit operator Key(ConfigKey key) {
      return key.UnderlyingKey;
    }

    public static Key operator /(ConfigKey parent, Key child) {
      return Key.Define(parent, child);
    }

    public static Key operator /(ConfigKey parent, string id) {
      return Key.Define(parent, id);
    }

    public string Id {
      get { return UnderlyingKey.Id; }
    }

    public string Description {
      get { return UnderlyingKey.Description; }
    }

    public ImmutableList<string> Path {
      get { return UnderlyingKey.Path; }
    }

    public bool IsRoot {
      get { return UnderlyingKey.IsRoot; }
    }

    public bool IsAbsolute {
      get { return UnderlyingKey.IsAbsolute; }
    }

    public int PathDepth {
      get { return UnderlyingKey.PathDepth; }
    }

    public Key Parent {
      get { return UnderlyingKey.Parent; }
    }

    public bool Equals(IKey otherKey) {
      return UnderlyingKey.Equals(otherKey);
    }

    public bool IdsEqual(IKey otherKey) {
      return UnderlyingKey.IdsEqual(otherKey);
    }

    public override int GetHashCode() {
      return UnderlyingKey.GetHashCode();
    }

    public override bool Equals(object obj) {
      return obj != null && obj.Equals(UnderlyingKey);
    }

    public override string ToString() {
      return UnderlyingKey.ToString();
    }
  }

  public class ConfigKey<T> : ConfigKey {
    public static implicit operator ConfigKey<T>(Key key) {
      return new ConfigKey<T>(key);
    }

    public static implicit operator Key(ConfigKey<T> key) {
      return key.UnderlyingKey;
    }

    private ConfigKey(Key underlyingKey) : base(underlyingKey) {}

    public static ConfigKey<T> operator /(Key parent, ConfigKey<T> child) {
      return Key.Define(parent, child);
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