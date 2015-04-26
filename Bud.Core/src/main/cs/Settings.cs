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

    public Settings AddIn(Key newScope, params Setup[] setups) => In(newScope).Add(setups).In(Scope);

    public Settings Add(params Setup[] setups) => Add((IEnumerable<Setup>) setups);

    public Settings Add(IEnumerable<Setup> setups) => setups.Aggregate(this, (oldSettings, plugin) => plugin(oldSettings)).In(Scope);

    public Settings AddGlobally(params Setup[] setups) => AddIn(Key.Root, setups);

    public static Settings Create(params Setup[] setups) => Empty.Add(setups);

    public Settings Add(ConfigModifier configModifier) => new Settings(ConfigModifiers.Add(configModifier), TaskModifiers, Scope);

    public Settings Add(TaskModifier taskModifier) => new Settings(ConfigModifiers, TaskModifiers.Add(taskModifier), Scope);

    public Settings Add(Settings settings) => new Settings(ConfigModifiers.AddRange(settings.ConfigModifiers), TaskModifiers.AddRange(settings.TaskModifiers), Scope);

    public ImmutableDictionary<ConfigKey, IConfigDefinition> ConfigDefinitions => ConfigModifiers.CompileToImmutableDictionary();

    public ImmutableDictionary<TaskKey, ITaskDefinition> TaskDefinitions => TaskModifiers.CompileToImmutableDictionary();

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