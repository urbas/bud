using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;
using Bud.Commander;
using NuGet;

namespace Bud.Plugins.NuGet {
  public static class NuGetKeys {
    public static readonly ConfigKey<ImmutableList<Scope>> ScopesWithNuGetDependencies = new ConfigKey<ImmutableList<Scope>>("ScopesWithNuGetDependencies");
    public static readonly ConfigKey<string> NuGetRepositoryDir = new ConfigKey<string>("NuGetRepositoryDir");
    public static readonly ConfigKey<ImmutableList<NuGetDependency>> NuGetDependencies = new ConfigKey<ImmutableList<NuGetDependency>>("NuGetDependencies");
    public static readonly TaskKey<ImmutableDictionary<string, IPackage>> ResolveNuGetDependencies = new TaskKey<ImmutableDictionary<string, IPackage>>("ResolveNuGetDependencies");
  }
}

