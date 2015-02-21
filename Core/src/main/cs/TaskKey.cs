using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Keys;
using Bud.SettingsConstruction;
using Bud.Util;

namespace Bud {
  public class TaskKey : Key {
    public new static TaskKey Define(string id, string description = null) {
      return KeyCreator.Define(id, description, TaskKeyFactory.Instance);
    }

    public static TaskKey Define(Key parentKey, TaskKey childKey) {
      return KeyCreator.Define(parentKey, childKey, TaskKeyFactory.Instance);
    }

    public new static TaskKey Define(Key parentKey, string id, string description = null) {
      return KeyCreator.Define(parentKey, id, description, TaskKeyFactory.Instance);
    }

    public new static TaskKey Define(ImmutableList<string> path, string description = null) {
      return KeyCreator.Define(path, description, TaskKeyFactory.Instance);
    }

    protected internal TaskKey(ImmutableList<string> path, string description) : base(path, description) {}

    public Setup Init(Func<IContext, Task> taskDefinition) {
      return settings => settings.Add(new InitializeTask(settings.Scope / this, taskDefinition));
    }

    public Setup InitSync(Action taskDefinition) {
      return settings => settings.Add(new InitializeTask(settings.Scope / this, ctxt => {
        taskDefinition();
        return TaskUtils.NullAsyncResult;
      }));
    }

    public Setup InitSync(Action<IContext> taskDefinition) {
      return settings => settings.Add(new InitializeTask(settings.Scope / this, ctxt => {
        taskDefinition(ctxt);
        return TaskUtils.NullAsyncResult;
      }));
    }

    public Setup InitSync(Action<IContext, Key> taskDefinition) {
      return settings => settings.Add(new InitializeTask(settings.Scope / this, ctxt => {
        taskDefinition(ctxt, settings.Scope);
        return TaskUtils.NullAsyncResult;
      }));
    }

    public Setup Init(Func<Task> taskDefinition) {
      return settings => settings.Add(new InitializeTask(settings.Scope / this, ctxt => taskDefinition()));
    }

    public Setup Init(Func<IContext, Key, Task> taskDefinition) {
      return settings => settings.Add(new InitializeTask(settings.Scope / this, ctxt => taskDefinition(ctxt, settings.Scope)));
    }

    public Setup Modify(Func<IContext, Func<Task>, Task> taskDefinition) {
      return settings => settings.Add(new ModifyTask(settings.Scope / this, taskDefinition));
    }

    public Setup Modify(Func<IContext, Func<Task>, Key, Task> newTaskDefinition) {
      return settings => settings.Add(new ModifyTask(settings.Scope / this, (ctxt, oldTaskDef) => newTaskDefinition(ctxt, oldTaskDef, settings.Scope)));
    }

    public Setup Modify(Func<Func<Task>, Task> newTaskDefinition) {
      return settings => settings.Add(new ModifyTask(settings.Scope / this, (ctxt, oldTaskDef) => newTaskDefinition(oldTaskDef)));
    }

    public Setup DependsOn(params TaskKey[] dependents) {
      return settings => settings.Add(new AddDependencies(settings.Scope / this, dependents));
    }

    public static TaskKey operator /(Key parent, TaskKey child) {
      return Define(parent, child);
    }
  }

  /// <summary>
  ///   Values of this key are evaluated once per evaluation context.
  /// </summary>
  public class TaskKey<T> : TaskKey {
    public new static TaskKey<T> Define(string id, string description = null) {
      return KeyCreator.Define(id, description, TaskKeyFactory<T>.Instance);
    }

    public static TaskKey<T> Define(Key parentKey, TaskKey<T> childKey) {
      return KeyCreator.Define(parentKey, childKey, TaskKeyFactory<T>.Instance);
    }

    public new static TaskKey<T> Define(Key parentKey, string id, string description = null) {
      return KeyCreator.Define(parentKey, id, description, TaskKeyFactory<T>.Instance);
    }

    public new static TaskKey<T> Define(ImmutableList<string> path, string description = null) {
      return KeyCreator.Define(path, description, TaskKeyFactory<T>.Instance);
    }

    internal TaskKey(ImmutableList<string> path, string description) : base(path, description) {}


    public static TaskKey<T> operator /(Key parent, TaskKey<T> child) {
      return Define(parent, child);
    }

    public Setup InitSync(Func<T> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(settings.Scope / this, ctxt => Task.FromResult(taskDefinition())));
    }

    public Setup InitSync(Func<IContext, T> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(settings.Scope / this, ctxt => Task.FromResult(taskDefinition(ctxt))));
    }

    public Setup InitSync(T taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(settings.Scope / this, ctxt => Task.FromResult(taskDefinition)));
    }

    public Setup InitSync(Func<IContext, Key, T> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(settings.Scope / this, ctxt => Task.FromResult(taskDefinition(ctxt, settings.Scope))));
    }

    public Setup Init(Func<Task<T>> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(settings.Scope / this, ctxt => taskDefinition()));
    }

    public Setup Init(Func<IContext, Task<T>> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(settings.Scope / this, taskDefinition));
    }

    public Setup Init(Func<IContext, Key, Task<T>> taskDefinition) {
      return settings => settings.Add(new InitializeTask<T>(settings.Scope / this, ctxt => taskDefinition(ctxt, settings.Scope)));
    }

    public Setup Modify(Func<IContext, Func<Task<T>>, Task<T>> taskDefinition) {
      return settings => settings.Add(new ModifyTask<T>(settings.Scope / this, taskDefinition));
    }

    public Setup Modify(Func<IContext, Func<Task<T>>, Key, Task<T>> newTaskDefinition) {
      return settings => settings.Add(new ModifyTask<T>(settings.Scope / this, (ctxt, oldTaskDef) => newTaskDefinition(ctxt, oldTaskDef, settings.Scope)));
    }

    public Setup Modify(Func<Func<Task<T>>, Task<T>> newTaskDefinition) {
      return settings => settings.Add(new ModifyTask<T>(settings.Scope / this, (ctxt, oldTaskDef) => newTaskDefinition(oldTaskDef)));
    }
  }
}