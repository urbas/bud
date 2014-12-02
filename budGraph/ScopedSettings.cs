using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bud
{
  public class ScopedSettings
  {
    public readonly Scope Scope;
    public readonly ImmutableList<Setting> Settings;

    public ScopedSettings(ImmutableList<Setting> settings, Scope scope) {
      this.Settings = settings;
      this.Scope = scope;
    }

    public static implicit operator Settings(ScopedSettings scopedSettings)
    {
      return new Bud.Settings(scopedSettings.Settings);
    }

    private ScopedSettings Add(Setting setting) {
      return new ScopedSettings(Settings.Add(setting), Scope);
    }

    public ScopedSettings Initialize<T>(ConfigKey<T> key, T initialValue) {
      return Add(new InitializeConfig<T>(key.In(Scope), initialValue));
    }

    public ScopedSettings Initialize<T>(ConfigKey<T> key, Func<EvaluationContext, Scope, T> initialValue) {
      return Add(new InitializeConfig<T>(key.In(Scope), context => initialValue(context, Scope)));
    }

    public ScopedSettings Initialize<T>(TaskKey<T> key, Func<EvaluationContext, Scope, T> task) {
      return Add(new InitializeTask<T>(key.In(Scope), context => Task.FromResult(task(context, Scope))));
    }

    public ScopedSettings InitAsync<T>(TaskKey<T> key, Func<EvaluationContext, Scope, Task<T>> task) {
      return Add(new InitializeTask<T>(key.In(Scope), context => task(context, Scope)));
    }

    public ScopedSettings EnsureInitialized<T>(ConfigKey<T> key, T initialValue) {
      return Add(new EnsureConfigInitialized<T>(key.In(Scope), initialValue));
    }

    public ScopedSettings EnsureInitialized<T>(TaskKey<T> key, Func<EvaluationContext, Scope, T> task) {
      return Add(new EnsureTaskInitialized<T>(key.In(Scope), new TaskDefinition<T>(context => Task.FromResult(task(context, Scope)))));
    }

    public ScopedSettings EnsureInitialized<T>(TaskKey<T> key, Func<EvaluationContext, Scope, Task<T>> task) {
      return Add(new EnsureTaskInitialized<T>(key.In(Scope), new TaskDefinition<T>(context => task(context, Scope))));
    }

    public ScopedSettings Modify<T>(ConfigKey<T> key, Func<T, T> modifier) {
      return Add(new ModifyConfig<T>(key.In(Scope), (context, previousValue) => modifier(previousValue)));
    }

    public ScopedSettings Modify<T>(TaskKey<T> key, Func<EvaluationContext, Scope, Func<Task<T>>, Task<T>> modifier) {
      return Add(new ModifyTask<T>(key.In(Scope), (context, previousTask) => modifier(context, Scope, previousTask)));
    }

    public ScopedSettings AddDependencies<T>(TaskKey<T> key, params TaskKey[] dependencies) {
      return Add(new AddDependencies<T>(key.In(Scope), dependencies));
    }

    public ScopedSettings AddDependencies<T>(TaskKey<T> key, Func<Scope, IEnumerable<TaskKey>> dependencies) {
      return Add(new AddDependencies<T>(key.In(Scope), dependencies(Scope)));
    }

    public ScopedSettings Globally(Func<Settings, Settings> settingsModifier) {
      return settingsModifier(new Settings(Settings)).ScopedTo(Scope);
    }

    public EvaluationContext ToEvaluationContext() {
      return EvaluationContext.ToEvaluationContext(this);
    }
	}
}

