using System;
using System.Linq;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bud {
  public class Settings {
    public static readonly Settings Start = new Settings(ImmutableList.Create<Setting>());

    public readonly Scope CurrentScope;
    public readonly ImmutableList<Setting> SettingsList;

    public Settings(ImmutableList<Setting> settings) : this(settings, Scope.Global) {
    }

    public Settings(ImmutableList<Setting> settings, Scope currentScope) {
      this.SettingsList = settings;
      this.CurrentScope = currentScope;
    }

    public Settings Add(Setting setting) {
      return new Settings(SettingsList.Add(setting), CurrentScope);
    }

    public Settings Add(Settings settings) {
      return new Settings(SettingsList.AddRange(settings.SettingsList));
    }

    public Settings Add(BudPlugin plugin) {
      return plugin.ApplyTo(this, CurrentScope);
    }

    public Settings Init<T>(ConfigKey<T> key, T initialValue) {
      return Add(new InitializeConfig<T>(key, initialValue));
    }

    public Settings Init<T>(ConfigKey<T> key, Func<EvaluationContext, T> initialValue) {
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

    public Settings InitOrKeep<T>(TaskKey<T> key, Func<EvaluationContext, T> task) {
      return Add(new EnsureTaskInitialized<T>(key, new TaskDefinition<T>(context => Task.FromResult(task(context)))));
    }

    public Settings InitOrKeep<T>(TaskKey<T> key, Func<EvaluationContext, Task<T>> task) {
      return Add(new EnsureTaskInitialized<T>(key, new TaskDefinition<T>(task)));
    }

    public Settings Modify<T>(ConfigKey<T> key, Func<T, T> modifier) {
      return Add(new ModifyConfig<T>(key, (context, previousValue) => modifier(previousValue)));
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
      return new Settings(SettingsList, scope);
    }

    public EvaluationContext ToEvaluationContext() {
      return EvaluationContext.ToEvaluationContext(this);
    }
  }
}