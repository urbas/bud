using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Dependencies {
  public class DependenciesPlugin : IPlugin {

    public static readonly DependenciesPlugin Instance = new DependenciesPlugin();

    private DependenciesPlugin() {
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .InitOrKeep(DependenciesKeys.Dependencies, ImmutableDictionary<Scope, Dependency>.Empty)
        .InitOrKeep(DependenciesKeys.DependencySources, ImmutableDictionary<Scope, DependencySource>.Empty);
    }

  }
}

