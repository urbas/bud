using System;
using System.Collections.Generic;
using System.Linq;

namespace Bud.SettingsConstruction {
  public class AddDependencies : TaskModifier {
    private readonly IEnumerable<TaskKey> ExtraDependencies;

    public AddDependencies(TaskKey key, IEnumerable<TaskKey> extraDependencies) : base(key) {
      ExtraDependencies = extraDependencies.Where(dependency => !dependency.Equals(key));
    }

    public override void Modify(IDictionary<TaskKey, ITaskDefinition> buildConfigurationBuilder) {
      ITaskDefinition value;
      if (buildConfigurationBuilder.TryGetValue(Key, out value)) {
        buildConfigurationBuilder[Key] = value.WithDependencies(ExtraDependencies);
      } else {
        throw new InvalidOperationException($"Cannot add dependencies to the task '{Key}'. This task has not yet been defined.");
      }
    }
  }
}