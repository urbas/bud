using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableList<InternalDependency>> InternalDependencies = new ConfigKey<ImmutableList<InternalDependency>>("InternalDependencies");
    public static readonly ConfigKey<ImmutableList<ExternalDependency>> ExternalDependencies = new ConfigKey<ImmutableList<ExternalDependency>>("ExternalDependencies");
    public static readonly TaskKey<ImmutableList<InternalDependency>> ResolveInternalDependencies = new TaskKey<ImmutableList<InternalDependency>>("ResolveInternalDependencies");
  }
}

