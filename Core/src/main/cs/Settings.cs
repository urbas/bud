using System.Collections.Immutable;
using System.Linq;
using Bud.SettingsConstruction;

namespace Bud {
  public class Settings {
    public static readonly Settings Empty = new Settings();
    public readonly Key Scope;
    public readonly ImmutableList<ConfigDefinitionConstructor> ConfigConstructors;
    public readonly ImmutableList<TaskDefinitionConstructor> TaskConstructors;

    public Settings() : this(Key.Global) {}

    public Settings(Key scope) : this(ImmutableList<ConfigDefinitionConstructor>.Empty, ImmutableList<TaskDefinitionConstructor>.Empty, scope) {}

    public Settings(ImmutableList<ConfigDefinitionConstructor> configConstructors, ImmutableList<TaskDefinitionConstructor> taskConstructors, Key scope) {
      ConfigConstructors = configConstructors;
      TaskConstructors = taskConstructors;
      Scope = scope;
    }

    public Settings In(Key newScope, params Setup[] setups) {
      return In(newScope).Do(setups).In(Scope);
    }

    public Settings In(Key newScope, Setup setup, Setup[] setups) {
      return In(newScope, setup).In(newScope, setups);
    }

    public Settings Do(params Setup[] setups) {
      return setups.Aggregate(this, (oldSettings, plugin) => plugin(oldSettings)).In(Scope);
    }

    public static Settings Create(params Setup[] setups) {
      return Empty.Do(setups);
    }

    public static Settings Create(Key scope, params Setup[] setups) {
      return Empty.In(scope, setups);
    }

    public Settings Add(ConfigDefinitionConstructor configConstructor) {
      return new Settings(ConfigConstructors.Add(configConstructor), TaskConstructors, Scope);
    }

    public Settings Add(TaskDefinitionConstructor taskConstructor) {
      return new Settings(ConfigConstructors, TaskConstructors.Add(taskConstructor), Scope);
    }

    public Settings Add(Settings settings) {
      return new Settings(ConfigConstructors.AddRange(settings.ConfigConstructors), TaskConstructors.AddRange(settings.TaskConstructors), Scope);
    }

    public ImmutableDictionary<Key, IConfigDefinition> ConfigDefinitions {
      get {
        var configDefinitions = ImmutableDictionary.CreateBuilder<Key, IConfigDefinition>();
        foreach (var configConstructor in ConfigConstructors) {
          configConstructor.ApplyTo(configDefinitions);
        }
        return configDefinitions.ToImmutable();
      }
    }

    public ImmutableDictionary<Key, ITaskDefinition> TaskDefinitions {
      get {
        var taskDefinitions = ImmutableDictionary.CreateBuilder<Key, ITaskDefinition>();
        foreach (var taskConstructor in TaskConstructors) {
          taskConstructor.ApplyTo(taskDefinitions);
        }
        return taskDefinitions.ToImmutable();
      }
    }

    private Settings In(Key newScope) {
      return newScope.Equals(Scope) ? this : new Settings(ConfigConstructors, TaskConstructors, newScope);
    }
  }
}