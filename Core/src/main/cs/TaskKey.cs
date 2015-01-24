using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bud.SettingsConstruction;

namespace Bud {
  public class TaskKey : Key {
    protected TaskKey(string id) : base(id) {}
    protected TaskKey(string id, Key parent) : base(id, parent) {}

    public Func<Settings, Settings> Init(Func<IContext, Task> taskDefinition) {
      return settings => settings.Add(new InitializeTask(In(settings.Scope), taskDefinition));
    }

    public Func<Settings, Settings> DependsOn(params TaskKey[] dependents) {
      return settings => settings.Add(new AddDependencies(In(settings.Scope), dependents));
    }

    public new TaskKey In(Key parent) {
      if (parent.IsGlobal) {
        return this;
      }
      return new TaskKey(Id, Concat(parent, Parent));
    }
  }

  /// <summary>
  ///   Values of this key are evaluated once per evaluation context.
  /// </summary>
  public class TaskKey<T> : TaskKey {
    public TaskKey(string id) : base(id) {}

    private TaskKey(string id, Key parent) : base(id, parent) {}

    public new TaskKey<T> In(Key parent) {
      if (parent.IsGlobal) {
        return this;
      }
      return new TaskKey<T>(Id, Concat(parent, Parent));
    }

    public Func<Settings, Settings> Init(Func<IContext, Task<T>> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(In(settings.Scope), taskDefinition));
    }

    public Func<Settings, Settings> Init(Func<IContext, Key, Task<T>> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(In(settings.Scope), ctxt => taskDefinition(ctxt, settings.Scope)));
    }

    public Func<Settings, Settings> Modify(Func<IContext, Func<Task<T>>, Task<T>> taskDefinition) {
      return settings => settings.Add(new ModifyTask<T>(In(settings.Scope), taskDefinition));
    }

    public Func<Settings, Settings> Modify(Func<IContext, Func<Task<T>>, Key, Task<T>> newTaskDefinition) {
      return settings => settings.Add(new ModifyTask<T>(In(settings.Scope), (ctxt, oldTaskDef) => newTaskDefinition(ctxt, oldTaskDef, settings.Scope)));
    }
  }
}