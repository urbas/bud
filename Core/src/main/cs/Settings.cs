using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.SettingsConstruction;

namespace Bud {
  public delegate Settings SettingsTransform(Settings existingSettings, Key key);

  public class Settings {
    public static readonly Settings Empty = new Settings();
    public readonly Key Scope;
    public readonly ImmutableList<ConfigDefinitionConstructor> ConfigConstructors;
    public readonly ImmutableList<TaskDefinitionConstructor> TaskConstructors;

    public Settings() : this(Key.Global) {}

    public Settings(Key scope) : this(ImmutableList<ConfigDefinitionConstructor>.Empty, ImmutableList<TaskDefinitionConstructor>.Empty, scope) {}

    public Settings(ImmutableList<ConfigDefinitionConstructor> configConstructors, ImmutableList<TaskDefinitionConstructor> taskConstructors, Key scope) {
      ConfigConstructors = configConstructors;
      TaskConstructors = taskConstructors;
      Scope = scope;
    }

    public Settings In(Key newScope, params Func<Settings, Settings>[] settingsTransforms) {
      return In(newScope).Do(settingsTransforms).In(Scope);
    }

    public Settings Do(params Func<Settings, Settings>[] settingsTransformations) {
      return settingsTransformations.Aggregate(this, (oldSettings, settingsTransform) => settingsTransform(oldSettings)).In(Scope);
    }

    public static Settings Create(params Func<Settings, Settings>[] settingTransformations) {
      return Empty.Do(settingTransformations);
    }

    public static Settings Create(Key scope, params Func<Settings, Settings>[] settingTransformations) {
      return Empty.In(scope, settingTransformations);
    }

    public Settings Add(ConfigDefinitionConstructor configConstructor) {
      return new Settings(ConfigConstructors.Add(configConstructor), TaskConstructors, Scope);
    }

    public Settings Add(TaskDefinitionConstructor taskConstructor) {
      return new Settings(ConfigConstructors, TaskConstructors.Add(taskConstructor), Scope);
    }

    public Settings Add(Settings settings) {
      return new Settings(ConfigConstructors.AddRange(settings.ConfigConstructors), TaskConstructors.AddRange(settings.TaskConstructors), Scope);
    }

    public Settings Apply(Key scope, IPlugin plugin) {
      return plugin.ApplyTo(this, scope);
    }

    public Settings Apply(Key scope, IPlugin plugin, params IPlugin[] plugins) {
      return Apply(scope, plugin).Apply(scope, plugins);
    }

    public Settings Apply(Key scope, IEnumerable<IPlugin> plugins) {
      return plugins.Aggregate(this, (settings, plugin) => settings.Apply(scope, plugin));
    }

    public Settings Modify<T>(ConfigKey<T> key, Func<T, T> modifier) {
      return Add(new ModifyConfig<T>(key.In(Scope), (context, previousValue) => modifier(previousValue)));
    }

    public Settings Modify<T>(ConfigKey<T> key, Func<IConfig, T, T> modifier) {
      return Add(new ModifyConfig<T>(key.In(Scope), (context, previousValue) => modifier(context, previousValue)));
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions {
      get {
        var configDefinitions = ImmutableDictionary.CreateBuilder<Key, IConfigDefinition>();
        foreach (var configConstructor in ConfigConstructors) {
          configConstructor.ApplyTo(configDefinitions);
        }
        return configDefinitions.ToImmutable();
      }
    }

    public ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions {
      get {
        var taskDefinitions = ImmutableDictionary.CreateBuilder<Key, ITaskDefinition>();
        foreach (var taskConstructor in TaskConstructors) {
          taskConstructor.ApplyTo(taskDefinitions);
        }
        return taskDefinitions.ToImmutable();
      }
    }

    private Settings In(Key newScope) {
      return newScope.Equals(Scope) ? this : new Settings(ConfigConstructors, TaskConstructors, newScope);
    }
  }
}