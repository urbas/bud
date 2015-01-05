using System;
using System.Linq;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bud {

  public delegate Settings SettingsTransform(Settings existingSettings, Key key);

  public class Settings {

    public static readonly Settings Empty = new Settings();
    public readonly ImmutableList<ConfigDefinitionConstructor> ConfigConstructors;
    public readonly ImmutableList<TaskDefinitionConstructor> TaskConstructors;

    public Settings() : this(ImmutableList<ConfigDefinitionConstructor>.Empty, ImmutableList<TaskDefinitionConstructor>.Empty) {}

    public Settings(ImmutableList<ConfigDefinitionConstructor> configConstructors, ImmutableList<TaskDefinitionConstructor> taskConstructors) {
      this.ConfigConstructors = configConstructors;
      this.TaskConstructors = taskConstructors;
    }

    public Settings Add(ConfigDefinitionConstructor configConstructor) {
      return new Settings(ConfigConstructors.Add(configConstructor), TaskConstructors);
    }

    public Settings Add(TaskDefinitionConstructor taskConstructor) {
      return new Settings(ConfigConstructors, TaskConstructors.Add(taskConstructor));
    }

    public Settings Add(Settings settings) {
      return new Settings(ConfigConstructors.AddRange(settings.ConfigConstructors), TaskConstructors.AddRange(settings.TaskConstructors));
    }

    public Settings Apply(Key key, IPlugin plugin) {
      return plugin.ApplyTo(this, key);
    }

    public Settings Init<T>(ConfigKey<T> key, T initialValue) {
      return Add(new InitializeConfig<T>(key, initialValue));
    }

    public Settings Init<T>(ConfigKey<T> key, Func<IConfig, T> initialValue) {
      return Add(new InitializeConfig<T>(key, initialValue));
    }

    public Settings Init<T>(TaskKey<T> key, Func<IContext, T> initialValue) {
      return Add(new InitializeTask<T>(key, context => Task.FromResult(initialValue(context))));
    }

    public Settings Init<T>(TaskKey<T> key, Func<IContext, Task<T>> task) {
      return Add(new InitializeTask<T>(key, task));
    }

    public Settings Modify<T>(ConfigKey<T> key, Func<T, T> modifier) {
      return Add(new ModifyConfig<T>(key, (context, previousValue) => modifier(previousValue)));
    }

    public Settings Modify<T>(ConfigKey<T> key, Func<IConfig, T, T> modifier) {
      return Add(new ModifyConfig<T>(key, (context, previousValue) => modifier(context, previousValue)));
    }

    public Settings Modify<T>(TaskKey<T> key, Func<IContext, Func<Task<T>>, Task<T>> modifier) {
      return Add(new ModifyTask<T>(key, modifier));
    }

    public Settings AddDependencies<T>(TaskKey<T> key, params TaskKey[] dependencies) {
      return Add(new AddDependencies<T>(key, dependencies));
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
  }
}