using System.Collections.Immutable;
using System.Linq;
using Bud.SettingsConstruction;

namespace Bud {
  public class Settings {
    public static readonly Settings Empty = new Settings();
    public readonly Key Scope;
    public readonly ImmutableList<ConfigModifier> ConfigModifiers;
    public readonly ImmutableList<TaskModifier> TaskModifier;

    public Settings() : this(Key.Root) {}

    public Settings(Key scope) : this(ImmutableList<ConfigModifier>.Empty, ImmutableList<TaskModifier>.Empty, scope) {}

    public Settings(ImmutableList<ConfigModifier> configModifiers, ImmutableList<TaskModifier> taskModifier, Key scope) {
      ConfigModifiers = configModifiers;
      TaskModifier = taskModifier;
      Scope = scope;
    }

    public Settings In(Key newScope, params Setup[] setups) {
      return In(newScope).Do(setups).In(Scope);
    }

    public Settings Do(params Setup[] setups) {
      return setups.Aggregate(this, (oldSettings, plugin) => plugin(oldSettings)).In(Scope);
    }

    public Settings Globally(params Setup[] setups) {
      return In(Key.Root).Do(setups).In(Scope);
    }

    public static Settings Create(params Setup[] setups) {
      return Empty.Do(setups);
    }

    public static Settings Create(Key scope, params Setup[] setups) {
      return Empty.In(scope, setups);
    }

    public static Setup Modify(params Setup[] setups) {
      return settings => settings.Do(setups);
    }

    public Settings Add(ConfigModifier configModifier) {
      return new Settings(ConfigModifiers.Add(configModifier), TaskModifier, Scope);
    }

    public Settings Add(TaskModifier taskModifier) {
      return new Settings(ConfigModifiers, TaskModifier.Add(taskModifier), Scope);
    }

    public Settings Add(Settings settings) {
      return new Settings(ConfigModifiers.AddRange(settings.ConfigModifiers), TaskModifier.AddRange(settings.TaskModifier), Scope);
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
        foreach (var taskConstructor in TaskModifier) {
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
        return new Settings(ConfigModifiers, TaskModifier, newScope);
      }
      return new Settings(ConfigModifiers, TaskModifier, Scope / newScope);
    }
  }
}