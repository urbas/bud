using System;
using System.Threading.Tasks;
using Bud.SettingsConstruction;
using Bud.Util;

namespace Bud {
  public class TaskKey : Key {
    public TaskKey(string id) : base(id) {}
    public TaskKey(string id, Key parent) : base(id, parent) {}

    public Setup Init(Func<IContext, Task> taskDefinition) {
      return settings => settings.Add(new InitializeTask(In(settings.Scope), taskDefinition));
    }

    public Setup InitSync(Action taskDefinition) {
      return settings => settings.Add(new InitializeTask(In(settings.Scope), ctxt => {
        taskDefinition();
        return TaskUtils.NullAsyncResult;
      }));
    }

    public Setup InitSync(Action<IContext> taskDefinition) {
      return settings => settings.Add(new InitializeTask(In(settings.Scope), ctxt => {
        taskDefinition(ctxt);
        return TaskUtils.NullAsyncResult;
      }));
    }

    public Setup InitSync(Action<IContext, Key> taskDefinition) {
      return settings => settings.Add(new InitializeTask(In(settings.Scope), ctxt => {
        taskDefinition(ctxt, settings.Scope);
        return TaskUtils.NullAsyncResult;
      }));
    }

    public Setup Init(Func<Task> taskDefinition) {
      return settings => settings.Add(new InitializeTask(In(settings.Scope), ctxt => taskDefinition()));
    }

    public Setup Init(Func<IContext, Key, Task> taskDefinition) {
      return settings => settings.Add(new InitializeTask(In(settings.Scope), ctxt => taskDefinition(ctxt, settings.Scope)));
    }

    public Setup Modify(Func<IContext, Func<Task>, Task> taskDefinition) {
      return settings => settings.Add(new ModifyTask(In(settings.Scope), taskDefinition));
    }

    public Setup Modify(Func<IContext, Func<Task>, Key, Task> newTaskDefinition) {
      return settings => settings.Add(new ModifyTask(In(settings.Scope), (ctxt, oldTaskDef) => newTaskDefinition(ctxt, oldTaskDef, settings.Scope)));
    }

    public Setup Modify(Func<Func<Task>, Task> newTaskDefinition) {
      return settings => settings.Add(new ModifyTask(In(settings.Scope), (ctxt, oldTaskDef) => newTaskDefinition(oldTaskDef)));
    }

    public Setup DependsOn(params TaskKey[] dependents) {
      return settings => settings.Add(new AddDependencies(In(settings.Scope), dependents));
    }

    public new TaskKey In(Key parent) {
      return parent.IsGlobal ? this : new TaskKey(Id, Concat(parent, Parent));
    }
  }

  /// <summary>
  ///   Values of this key are evaluated once per evaluation context.
  /// </summary>
  public class TaskKey<T> : TaskKey {
    public TaskKey(string id) : base(id) {}

    private TaskKey(string id, Key parent) : base(id, parent) {}

    public new TaskKey<T> In(Key parent) {
      return parent.IsGlobal ? this : new TaskKey<T>(Id, Concat(parent, Parent));
    }

    public Setup InitSync(Func<T> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(In(settings.Scope), ctxt => Task.FromResult(taskDefinition())));
    }

    public Setup InitSync(Func<IContext, T> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(In(settings.Scope), ctxt => Task.FromResult(taskDefinition(ctxt))));
    }

    public Setup InitSync(T taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(In(settings.Scope), ctxt => Task.FromResult(taskDefinition)));
    }

    public Setup InitSync(Func<IContext, Key, T> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(In(settings.Scope), ctxt => Task.FromResult(taskDefinition(ctxt, settings.Scope))));
    }

    public Setup Init(Func<Task<T>> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(In(settings.Scope), ctxt => taskDefinition()));
    }

    public Setup Init(Func<IContext, Task<T>> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(In(settings.Scope), taskDefinition));
    }

    public Setup Init(Func<IContext, Key, Task<T>> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(In(settings.Scope), ctxt => taskDefinition(ctxt, settings.Scope)));
    }

    public Setup Modify(Func<IContext, Func<Task<T>>, Task<T>> taskDefinition) {
      return settings => settings.Add(new ModifyTask<T>(In(settings.Scope), taskDefinition));
    }

    public Setup Modify(Func<IContext, Func<Task<T>>, Key, Task<T>> newTaskDefinition) {
      return settings => settings.Add(new ModifyTask<T>(In(settings.Scope), (ctxt, oldTaskDef) => newTaskDefinition(ctxt, oldTaskDef, settings.Scope)));
    }

    public Setup Modify(Func<Func<Task<T>>, Task<T>> newTaskDefinition) {
      return settings => settings.Add(new ModifyTask<T>(In(settings.Scope), (ctxt, oldTaskDef) => newTaskDefinition(oldTaskDef)));
    }
  }
}