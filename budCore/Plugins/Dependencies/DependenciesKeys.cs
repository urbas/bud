using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableDictionary<Scope, Dependency>> Dependencies = new ConfigKey<ImmutableDictionary<Scope, Dependency>>("Dependencies");
    public static readonly ConfigKey<ImmutableDictionary<Scope, DependencySource>> DependencySources = new ConfigKey<ImmutableDictionary<Scope, DependencySource>>("DependencySources");
  }
}

