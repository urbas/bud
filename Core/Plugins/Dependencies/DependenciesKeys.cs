using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableList<IDependency>> Dependencies = new ConfigKey<ImmutableList<IDependency>>("Dependencies");
  }
}

