using System;
using System.Linq;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bud {

  public delegate Settings SettingApplication(Settings existingSettings, Scope toScope);

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

    public Settings ApplyGlobally(IPlugin plugin) {
      return plugin.ApplyTo(this, Scope.Global);
    }

    public Settings Apply(Scope toScope, IPlugin plugin) {
      return plugin.ApplyTo(this, toScope);
    }

    public Settings Init<T>(ConfigKey<T> key, T initialValue) {
      return Add(new InitializeConfig<T>(key, initialValue));
    }

    public Settings Init<T>(ConfigKey<T> key, Func<Configuration, T> initialValue) {
      return Add(new InitializeConfig<T>(key, initialValue));
    }

    public Settings Init<T>(TaskKey<T> key, Func<EvaluationContext, T> initialValue) {
      return Add(new InitializeTask<T>(key, context => Task.FromResult(initialValue(context))));
    }

    public Settings Init<T>(TaskKey<T> key, Func<EvaluationContext, Task<T>> task) {
      return Add(new InitializeTask<T>(key, task));
    }

    public Settings Modify<T>(ConfigKey<T> key, Func<T, T> modifier) {
      return Add(new ModifyConfig<T>(key, (context, previousValue) => modifier(previousValue)));
    }

    public Settings Modify<T>(ConfigKey<T> key, Func<Configuration, T, T> modifier) {
      return Add(new ModifyConfig<T>(key, (context, previousValue) => modifier(context, previousValue)));
    }

    public Settings Modify<T>(TaskKey<T> key, Func<EvaluationContext, Func<Task<T>>, Task<T>> modifier) {
      return Add(new ModifyTask<T>(key, modifier));
    }

    public Settings AddDependencies<T>(TaskKey<T> key, params TaskKey[] dependencies) {
      return Add(new AddDependencies<T>(key, dependencies));
    }

    public ScopeDefinitions Compile() {
      var configDefinitions = ImmutableDictionary.CreateBuilder<Scope, IConfigDefinition>();
      foreach (var configConstructor in ConfigConstructors) {
        configConstructor.ApplyTo(configDefinitions);
      }
      var taskDefinitions = ImmutableDictionary.CreateBuilder<Scope, ITaskDefinition>();
      foreach (var taskConstructor in TaskConstructors) {
        taskConstructor.ApplyTo(taskDefinitions);
      }
      return new ScopeDefinitions(configDefinitions.ToImmutable(), taskDefinitions.ToImmutable());
    }
  }
}