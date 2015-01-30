using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<ImmutableList<InternalDependency>> InternalDependencies = new ConfigKey<ImmutableList<InternalDependency>>("internalDependencies");
    public static readonly ConfigKey<ImmutableList<ExternalDependency>> ExternalDependencies = new ConfigKey<ImmutableList<ExternalDependency>>("externalDependencies");
    public static readonly TaskKey<ISet<Key>> ResolveInternalDependencies = new TaskKey<ISet<Key>>("resolveInternalDependencies");
    public static readonly ConfigKey<ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>>> ExternalDependenciesKeys = new ConfigKey<ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>>>("externalDependenciesKeys");
    public static readonly ConfigKey<string> NuGetRepositoryDir = new ConfigKey<string>("nuGetRepositoryDir");
    public static readonly ConfigKey<string> PersistedPackagesListFile = new ConfigKey<string>("persistedPackagesListFile");
    public static readonly ConfigKey<BudExternalPackageRepository> NuGetFetchedPackages = new ConfigKey<BudExternalPackageRepository>("nuGetFetchedPackages");
    public static readonly ConfigKey<ImmutableHashSet<IDependency>> Dependencies = new ConfigKey<ImmutableHashSet<IDependency>>("dependencies");
    public static readonly TaskKey<BudExternalPackageRepository> Fetch = new TaskKey<BudExternalPackageRepository>("fetch", "Installs NuGet packages specified as dependencies in the build definition. This method updates existing packages and the persisted list of installed packages.");
    public static readonly TaskKey CleanDependencies = new TaskKey("clean", "Deletes the downloaded NuGet packages and the persisted list of installed packages. You should re-fetch the packages after invoking this task.").In(Fetch);
  }
}