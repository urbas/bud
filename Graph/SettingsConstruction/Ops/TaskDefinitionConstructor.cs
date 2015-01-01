using System;
using Bud.SettingsConstruction;
using System.Collections.Immutable;

namespace Bud {
  public abstract class TaskDefinitionConstructor {
    public readonly TaskKey Key;

    public TaskDefinitionConstructor(TaskKey key) {
      Key = key;
    }

    public abstract void ApplyTo(ImmutableDictionary<Scope, ITaskDefinition>.Builder taskDefinitionBuilder);
  }
}

