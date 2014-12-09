using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableList<Dependency>> Dependencies = new ConfigKey<ImmutableList<Dependency>>("Dependencies");
  }
}

