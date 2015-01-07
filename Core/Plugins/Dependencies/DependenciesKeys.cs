using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableList<InternalDependency>> InternalDependencies = new ConfigKey<ImmutableList<InternalDependency>>("InternalDependencies");
    public static readonly TaskKey<ImmutableList<InternalDependency>> ResolveInternalDependencies = new TaskKey<ImmutableList<InternalDependency>>("ResolveInternalDependencies");
    public static readonly ConfigKey<ImmutableList<ExternalDependency>> ExternalDependencies = new ConfigKey<ImmutableList<ExternalDependency>>("ExternalDependencies");
    public static readonly ConfigKey<ImmutableList<ConfigKey<ImmutableList<ExternalDependency>>>> ExternalDependenciesKeys = new ConfigKey<ImmutableList<ConfigKey<ImmutableList<ExternalDependency>>>>("ExternalDependenciesKeys");
    public static readonly ConfigKey<string> NuGetRepositoryDir = new ConfigKey<string>("NuGetRepositoryDir");
    public static readonly TaskKey<NuGetResolution> Fetch = new TaskKey<NuGetResolution>("Fetch");
    public static readonly ConfigKey<NuGetResolution> NuGetResolvedPackages = new ConfigKey<NuGetResolution>("NuGetResolvedPackages");
  }
}

