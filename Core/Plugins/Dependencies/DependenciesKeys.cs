using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableList<IDependency>> Dependencies = new ConfigKey<ImmutableList<IDependency>>("Dependencies");
    public static readonly TaskKey<ImmutableList<IResolvedDependency>> ResolveDependencies = new TaskKey<ImmutableList<IResolvedDependency>>("ResolveDependencies");
    public static readonly TaskKey<IResolvedDependency> ResolveScopeDependency = new TaskKey<IResolvedDependency>("ResolveScopeDependency");
  }
}

