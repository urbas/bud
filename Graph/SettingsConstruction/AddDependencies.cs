﻿using System;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace Bud.SettingsConstruction {
  public class AddDependencies<T> : TaskDefinitionConstructor {
    private IEnumerable<TaskKey> extraDependencies;

    public AddDependencies(TaskKey<T> key, IEnumerable<TaskKey> extraDependencies) : base(key) {
      this.extraDependencies = extraDependencies;
    }

    public override void ApplyTo(ImmutableDictionary<Scope, ITaskDefinition>.Builder buildConfigurationBuilder) {
      ITaskDefinition value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        buildConfigurationBuilder[Key] = ((TaskDefinition<T>)value).WithDependencies(extraDependencies);
      } else {
        throw new InvalidOperationException(string.Format("Cannot add dependencies to the task '{0}'. This task has not yet been defined.", Key));
      }
    }
  }
}
