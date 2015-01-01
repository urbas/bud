using System;
using System.Collections.Immutable;
using Bud.SettingsConstruction;

namespace Bud {
  public class ScopeDefinitions {
    public readonly ImmutableDictionary<Scope, IConfigDefinition> ConfigDefinitions;
    public readonly ImmutableDictionary<Scope, ITaskDefinition> TaskDefinitions;

    public ScopeDefinitions(ImmutableDictionary<Scope, IConfigDefinition> configDefinitions, ImmutableDictionary<Scope, ITaskDefinition> taskDefinitions) {
      ConfigDefinitions = configDefinitions;
      TaskDefinitions = taskDefinitions;
    }
  }
}

