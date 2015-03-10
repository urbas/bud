using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.SettingsConstruction;
using Bud.Util;

namespace Bud {
  public class TaskKey : IKey {
    protected internal readonly Key UnderlyingKey;

    protected TaskKey(Key underlyingKey) {
      UnderlyingKey = underlyingKey;
    }

    public static implicit operator TaskKey(Key key) => new TaskKey(key);

    public static implicit operator Key(TaskKey key) => key.UnderlyingKey;

    public static Key operator /(TaskKey parent, Key child) => Key.Define(parent, child);

    public static Key operator /(TaskKey parent, string id) => Key.Define(parent, id);

    public static TaskKey operator /(Key parent, TaskKey child) => Key.Define(parent, child);

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

    public string Id => UnderlyingKey.Id;

    public string Description => UnderlyingKey.Description;

    public ImmutableList<string> PathComponents => UnderlyingKey.PathComponents;

    public bool IsRoot => UnderlyingKey.IsRoot;

    public bool IsAbsolute => UnderlyingKey.IsAbsolute;

    public Key Parent => UnderlyingKey.Parent;

    public Key Leaf => UnderlyingKey.Leaf;

    public string Path => UnderlyingKey.Path;

    public bool Equals(IKey otherKey) => UnderlyingKey.Equals(otherKey);

    public override int GetHashCode() => UnderlyingKey.GetHashCode();

    public override bool Equals(object obj) => obj != null && obj.Equals(UnderlyingKey);

    public override string ToString() => UnderlyingKey.ToString();
  }

  /// <summary>
  ///   Values of this key are evaluated once per evaluation context.
  /// </summary>
  public class TaskKey<T> : TaskKey {
    public static implicit operator TaskKey<T>(Key key) => new TaskKey<T>(key);

    public static implicit operator Key(TaskKey<T> key) => key.UnderlyingKey;

    private TaskKey(Key underlyingKey) : base(underlyingKey) {}

    public static TaskKey<T> operator /(Key parent, TaskKey<T> child) => Key.Define(parent, child);

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