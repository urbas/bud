using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Build;

namespace Bud.Dependencies {
  public static class DependenciesKeys {
    public static readonly ConfigKey<IEnumerable<IDependency>> Dependencies = Key.Define("dependencies", "The list of downloaded external dependencies of particular versions and internal dependencies of particular versions.");
    public static readonly ConfigKey<ImmutableList<InternalDependency>> InternalDependencies = Dependencies / "internal";
    public static readonly ConfigKey<ImmutableList<ExternalDependency>> ExternalDependencies = Dependencies / "external";
    public static readonly ConfigKey<ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>>> ExternalDependenciesKeys = ExternalDependencies / Key.Define("keys", "The aggregation of all external dependencies keys.");
    public static readonly TaskKey<ISet<Key>> EvaluateInternalDependencies = InternalDependencies / Key.Define("evaluate", "Evaluates the internal dependencies.");
    public static readonly ConfigKey<string> FetchedDependenciesDir = Dependencies / "fetchedDependenciesDir";
    public static readonly ConfigKey<string> FetchedDependenciesListFile = Dependencies / "fetchedDependenciesListFile";
    public static readonly ConfigKey<FetchedDependencies> FetchedDependencies = Key.Define("fetchedDependencies", string.Format("A list of downloaded external dependencies. This list can be committed to the versioning control system to ensure that projects are built against the same versions of external dependencies on every machine."));
    public static readonly TaskKey Fetch = Key.Define("fetch", $"Downloads the latest versions of external packages. It also updates the '{FetchedDependenciesListFile}' file.");
    public static readonly TaskKey CleanDependencies = Dependencies / Key.Define(BuildDirsKeys.Clean.Id, string.Format("Deletes the downloaded NuGet packages."));
  }
}