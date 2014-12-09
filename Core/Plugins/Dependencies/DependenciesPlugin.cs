using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Dependencies {
  public class DependenciesPlugin : IPlugin {

    public static readonly DependenciesPlugin Instance = new DependenciesPlugin();

    private DependenciesPlugin() {
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .InitOrKeep(DependenciesKeys.Dependencies.In(scope), ImmutableList<Dependency>.Empty);
    }

  }
}

