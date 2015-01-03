using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableList<TaskKey>> Dependencies = new ConfigKey<ImmutableList<TaskKey>>("Dependencies");
    public static readonly TaskKey<ImmutableList<TaskKey>> ResolveDependencies = new TaskKey<ImmutableList<TaskKey>>("ResolveDependencies");
  }
}

