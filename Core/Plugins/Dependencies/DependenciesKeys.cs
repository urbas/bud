using System;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace Bud.Plugins.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableList<InternalDependency>> InternalDependencies = new ConfigKey<ImmutableList<InternalDependency>>("InternalDependencies");
    public static readonly ConfigKey<ImmutableList<ExternalDependency>> ExternalDependencies = new ConfigKey<ImmutableList<ExternalDependency>>("ExternalDependencies");
    public static readonly TaskKey<ResolvedExternalDependencies> Fetch = new TaskKey<ResolvedExternalDependencies>("Fetch");
    public static readonly TaskKey<ISet<Key>> ResolveInternalDependencies = new TaskKey<ISet<Key>>("ResolveInternalDependencies");
    public static readonly ConfigKey<ImmutableList<ConfigKey<ImmutableList<ExternalDependency>>>> ExternalDependenciesKeys = new ConfigKey<ImmutableList<ConfigKey<ImmutableList<ExternalDependency>>>>("ExternalDependenciesKeys");
    public static readonly ConfigKey<string> NuGetRepositoryDir = new ConfigKey<string>("NuGetRepositoryDir");
    public static readonly ConfigKey<ResolvedExternalDependencies> NuGetResolvedPackages = new ConfigKey<ResolvedExternalDependencies>("NuGetResolvedPackages");
  }
}

