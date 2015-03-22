using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.SettingsConstruction;

namespace Bud {
  public class Settings {
    public static readonly Settings Empty = new Settings();
    public readonly Key Scope;
    public readonly ImmutableList<ConfigModifier> ConfigModifiers;
    public readonly ImmutableList<TaskModifier> TaskModifiers;

    private Settings() : this(Key.Root) {}

    private Settings(Key scope) : this(ImmutableList<ConfigModifier>.Empty, ImmutableList<TaskModifier>.Empty, scope) {}

    private Settings(ImmutableList<ConfigModifier> configModifiers, ImmutableList<TaskModifier> taskModifiers, Key scope) {
      ConfigModifiers = configModifiers;
      TaskModifiers = taskModifiers;
      Scope = scope;
    }

    public Settings AddIn(Key newScope, params Setup[] setups) {
      return In(newScope).Add(setups).In(Scope);
    }

    public Settings Add(params Setup[] setups) {
      return Add((IEnumerable<Setup>) setups);
    }

    public Settings Add(IEnumerable<Setup> setups) {
      return setups.Aggregate(this, (oldSettings, plugin) => plugin(oldSettings)).In(Scope);
    }

    public Settings AddGlobally(params Setup[] setups) {
      return In(Key.Root).Add(setups).In(Scope);
    }

    public static Settings Create(params Setup[] setups) {
      return Empty.Add(setups);
    }

    public Settings Add(ConfigModifier configModifier) {
      return new Settings(ConfigModifiers.Add(configModifier), TaskModifiers, Scope);
    }

    public Settings Add(TaskModifier taskModifier) {
      return new Settings(ConfigModifiers, TaskModifiers.Add(taskModifier), Scope);
    }

    public Settings Add(Settings settings) {
      return new Settings(ConfigModifiers.AddRange(settings.ConfigModifiers), TaskModifiers.AddRange(settings.TaskModifiers), Scope);
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions {
      get {
        var configDefinitions = ImmutableDictionary.CreateBuilder<Key, IConfigDefinition>();
        foreach (var configConstructor in ConfigModifiers) {
          configConstructor.ApplyTo(configDefinitions);
        }
        return configDefinitions.ToImmutable();
      }
    }

    public ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions {
      get {
        var taskDefinitions = ImmutableDictionary.CreateBuilder<Key, ITaskDefinition>();
        foreach (var taskConstructor in TaskModifiers) {
          taskConstructor.ApplyTo(taskDefinitions);
        }
        return taskDefinitions.ToImmutable();
      }
    }

    private Settings In(Key newScope) {
      if (newScope.IsAbsolute) {
        if (newScope.Equals(Scope)) {
          return this;
        }
        return new Settings(ConfigModifiers, TaskModifiers, newScope);
      }
      return new Settings(ConfigModifiers, TaskModifiers, Scope / newScope);
    }
  }
}