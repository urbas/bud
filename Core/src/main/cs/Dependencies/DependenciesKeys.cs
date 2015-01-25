﻿using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableList<InternalDependency>> InternalDependencies = new ConfigKey<ImmutableList<InternalDependency>>("internalDependencies");
    public static readonly ConfigKey<ImmutableList<ExternalDependency>> ExternalDependencies = new ConfigKey<ImmutableList<ExternalDependency>>("externalDependencies");
    public static readonly TaskKey<NuGetPackages> Fetch = new TaskKey<NuGetPackages>("fetch");
    public static readonly TaskKey<ISet<Key>> ResolveInternalDependencies = new TaskKey<ISet<Key>>("resolveInternalDependencies");
    public static readonly ConfigKey<ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>>> ExternalDependenciesKeys = new ConfigKey<ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>>>("externalDependenciesKeys");
    public static readonly ConfigKey<string> NuGetRepositoryDir = new ConfigKey<string>("nuGetRepositoryDir");
    public static readonly ConfigKey<NuGetPackages> NuGetResolvedPackages = new ConfigKey<NuGetPackages>("nuGetResolvedPackages");

    public static ConfigKey<ImmutableList<ExternalDependency>> GetExternalDependenciesKey(Key dependent) {
      return DependenciesKeys.ExternalDependencies.In(dependent);
    }

    public static ConfigKey<ImmutableList<InternalDependency>> GetInternalDependenciesKey(Key dependent) {
      return DependenciesKeys.InternalDependencies.In(dependent);
    }
  }
}