using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Build;

namespace Bud.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<IEnumerable<IDependency>> Dependencies = new ConfigKey<IEnumerable<IDependency>>("dependencies", "The list of downloaded external dependencies of particular versions and internal dependencies of particular versions.");
    public static readonly ConfigKey<ImmutableList<InternalDependency>> InternalDependencies = new ConfigKey<ImmutableList<InternalDependency>>("internal").In(Dependencies);
    public static readonly ConfigKey<ImmutableList<ExternalDependency>> ExternalDependencies = new ConfigKey<ImmutableList<ExternalDependency>>("external").In(Dependencies);
    public static readonly ConfigKey<ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>>> ExternalDependenciesKeys = new ConfigKey<ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>>>("keys", "The aggregation of all external dependencies keys.").In(ExternalDependencies);
    public static readonly TaskKey<ISet<Key>> EvaluateInternalDependencies = new TaskKey<ISet<Key>>("evaluate", "Evaluates the internal dependencies.").In(InternalDependencies);
    public static readonly ConfigKey<string> DependenciesRepositoryDir = new ConfigKey<string>("repositoryDir").In(Dependencies);
    public static readonly ConfigKey<string> FetchedDependenciesFile = new ConfigKey<string>("fetchedDependenciesFile").In(Dependencies);
    public static readonly ConfigKey<FetchedDependencies> FetchedDependencies = new ConfigKey<FetchedDependencies>("fetched", string.Format("A list of external dependencies to download. This list can be committed to the versioning control system to ensure that projects are built against the same versions of external dependencies on every machine."));
    public static readonly TaskKey<FetchedDependencies> FetchDependencies = new TaskKey<FetchedDependencies>("fetch", string.Format("Downloads the versions of external packages that are specified in '{0}'. If the file '{0}' does not exist, then the latest versions of external dependencies will be downloaded.", FetchedDependencies));
    public static readonly TaskKey<FetchedDependencies> UpdateDependencies = new TaskKey<FetchedDependencies>("update", "Downloads the latest versions of external packages.").In(Dependencies);
    public static readonly TaskKey CleanDependencies = new TaskKey(BuildDirsKeys.Clean.Id, string.Format("Deletes the downloaded NuGet packages. You should invoke either '{0}' or '{1}' to download the packages again.", FetchDependencies, UpdateDependencies)).In(Dependencies);
  }
}