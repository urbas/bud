using System;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace Bud.Plugins.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableList<InternalDependency>> InternalDependencies = new ConfigKey<ImmutableList<InternalDependency>>("internalDependencies");
    public static readonly ConfigKey<ImmutableList<ExternalDependency>> ExternalDependencies = new ConfigKey<ImmutableList<ExternalDependency>>("externalDependencies");
    public static readonly TaskKey<ResolvedExternalDependencies> Fetch = new TaskKey<ResolvedExternalDependencies>("fetch");
    public static readonly TaskKey<ISet<Key>> ResolveInternalDependencies = new TaskKey<ISet<Key>>("resolveInternalDependencies");
    public static readonly ConfigKey<ImmutableList<ConfigKey<ImmutableList<ExternalDependency>>>> ExternalDependenciesKeys = new ConfigKey<ImmutableList<ConfigKey<ImmutableList<ExternalDependency>>>>("externalDependenciesKeys");
    public static readonly ConfigKey<string> NuGetRepositoryDir = new ConfigKey<string>("nuGetRepositoryDir");
    public static readonly ConfigKey<ResolvedExternalDependencies> NuGetResolvedPackages = new ConfigKey<ResolvedExternalDependencies>("nuGetResolvedPackages");
  }
}

