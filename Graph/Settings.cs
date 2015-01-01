using System;
using System.Linq;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bud {
  public class Settings {
    public static readonly Settings Empty = new Settings();

    public readonly Scope CurrentScope;
    public readonly ImmutableList<ConfigDefinitionConstructor> ConfigConstructors;
    public readonly ImmutableList<TaskDefinitionConstructor> TaskConstructors;

    public Settings() : this(ImmutableList<ConfigDefinitionConstructor>.Empty, ImmutableList<TaskDefinitionConstructor>.Empty) {}

    public Settings(ImmutableList<ConfigDefinitionConstructor> configConstructors, ImmutableList<TaskDefinitionConstructor> taskConstructors) : this(configConstructors, taskConstructors, Scope.Global) {    }

    public Settings(ImmutableList<ConfigDefinitionConstructor> configConstructors, ImmutableList<TaskDefinitionConstructor> taskConstructors, Scope currentScope) {
      this.ConfigConstructors = configConstructors;
      this.TaskConstructors = taskConstructors;
      this.CurrentScope = currentScope;
    }

    public Settings Add(ConfigDefinitionConstructor configConstructor) {
      return new Settings(ConfigConstructors.Add(configConstructor), TaskConstructors, CurrentScope);
    }

    public Settings Add(TaskDefinitionConstructor taskConstructor) {
      return new Settings(ConfigConstructors, TaskConstructors.Add(taskConstructor), CurrentScope);
    }

    public Settings Add(Settings settings) {
      return new Settings(ConfigConstructors.AddRange(settings.ConfigConstructors), TaskConstructors.AddRange(settings.TaskConstructors));
    }

    public Settings Add(IPlugin plugin) {
      return plugin.ApplyTo(this, CurrentScope);
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

    public Settings InitOrKeep<T>(ConfigKey<T> key, T initialValue) {
      return Add(new EnsureConfigInitialized<T>(key, initialValue));
    }

    public Settings InitOrKeep<T>(ConfigKey<T> key, Func<Configuration, T> initialValue) {
      return Add(new EnsureConfigInitialized<T>(key, initialValue));
    }

    public Settings InitOrKeep<T>(TaskKey<T> key, Func<EvaluationContext, T> task) {
      return Add(new EnsureTaskInitialized<T>(key, new TaskDefinition<T>(context => Task.FromResult(task(context)))));
    }

    public Settings InitOrKeep<T>(TaskKey<T> key, Func<EvaluationContext, Task<T>> task) {
      return Add(new EnsureTaskInitialized<T>(key, new TaskDefinition<T>(task)));
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

    public Settings SetCurrentScope(Scope scope) {
      if (CurrentScope.Equals(scope)) {
        return this;
      }
      return new Settings(ConfigConstructors, TaskConstructors, scope);
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