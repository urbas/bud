using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.Util;

namespace Bud.SettingsConstruction {
  public class AddDependencies : TaskDefinitionConstructor {
    private IEnumerable<TaskKey> extraDependencies;

    public AddDependencies(TaskKey key, IEnumerable<TaskKey> extraDependencies) : base(key) {
      this.extraDependencies = extraDependencies.Where(dependency => !dependency.Equals(key));
    }

    public override void ApplyTo(ImmutableDictionary<Key, ITaskDefinition>.Builder buildConfigurationBuilder) {
      ITaskDefinition value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        buildConfigurationBuilder[Key] = value.WithDependencies(extraDependencies);
      } else {
        throw new InvalidOperationException(string.Format("Cannot add dependencies to the task '{0}'. This task has not yet been defined.", Key));
      }
    }
  }
}