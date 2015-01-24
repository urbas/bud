using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
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
      var scopedSettings = In(newScope);
      return settingsTransforms.Aggregate(scopedSettings, (oldSettings, settingsTransform) => settingsTransform(oldSettings)).In(Scope);
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

    public Settings Init<T>(ConfigKey<T> key, T initialValue) {
      return Add(new InitializeConfig<T>(key.In(Scope), initialValue));
    }

    public Settings Init<T>(ConfigKey<T> key, Func<IConfig, T> initialValue) {
      return Add(new InitializeConfig<T>(key.In(Scope), initialValue));
    }

    public Settings Init<T>(TaskKey<T> key, Func<IContext, T> initialValue) {
      return Add(new InitializeTask<T>(key.In(Scope), context => Task.FromResult(initialValue(context))));
    }

    public Settings Init<T>(TaskKey<T> key, Func<IContext, Task<T>> task) {
      return Add(new InitializeTask<T>(key, task));
    }

    public Settings Modify<T>(ConfigKey<T> key, Func<T, T> modifier) {
      return Add(new ModifyConfig<T>(key.In(Scope), (context, previousValue) => modifier(previousValue)));
    }

    public Settings Modify<T>(ConfigKey<T> key, Func<IConfig, T, T> modifier) {
      return Add(new ModifyConfig<T>(key.In(Scope), (context, previousValue) => modifier(context, previousValue)));
    }

    public Settings Modify<T>(TaskKey<T> key, Func<IContext, Func<Task<T>>, Task<T>> modifier) {
      return Add(new ModifyTask<T>(key.In(Scope), modifier));
    }

    public Settings AddDependencies(TaskKey key, params TaskKey[] dependencies) {
      return Add(new AddDependencies(key.In(Scope), dependencies));
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
      return new Settings(ConfigConstructors, TaskConstructors, newScope);
    }
  }
}