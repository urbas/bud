using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableList<ScopeDependency>> ScopeDependencies = new ConfigKey<ImmutableList<ScopeDependency>>("ScopeDependencies");
    public static readonly TaskKey<ImmutableList<ResolvedScopeDependency>> ResolveScopeDependencies = new TaskKey<ImmutableList<ResolvedScopeDependency>>("ResolveScopeDependencies");
    public static readonly TaskKey<ResolvedScopeDependency> ResolveScopeDependency = new TaskKey<ResolvedScopeDependency>("ResolveScopeDependency");
  }
}

