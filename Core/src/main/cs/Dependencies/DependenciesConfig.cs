using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Build;

namespace Bud.Dependencies {
  public static class DependenciesConfig {
    public static ImmutableList<InternalDependency> GetInternalDependencies(this IConfig config, Key dependent) {
      var dependenciesKey = DependenciesKeys.InternalDependencies.In(dependent);
      return config.IsConfigDefined(dependenciesKey) ? config.Evaluate(dependenciesKey) : ImmutableList<InternalDependency>.Empty;
    }

    public static ImmutableList<ExternalDependency> GetExternalDependencies(this IConfig config, Key dependent) {
      var dependenciesKey = DependenciesKeys.ExternalDependencies.In(dependent);
      return config.IsConfigDefined(dependenciesKey) ? config.Evaluate(dependenciesKey) : ImmutableList<ExternalDependency>.Empty;
    }

    public static ImmutableList<ExternalDependency> GetExternalDependencies(this IConfig context) {
      var keysWithNuGetDependencies = context.GetKeysWithExternalDependencies();
      return keysWithNuGetDependencies.SelectMany(context.Evaluate).ToImmutableList();
    }

    private static ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>> GetKeysWithExternalDependencies(this IConfig context) {
      return context.Evaluate(DependenciesKeys.ExternalDependenciesKeys);
    }

    /// <summary>
    /// Invokes the tasks on all internal dependencies and returns
    /// </summary>
    /// <param name="context"></param>
    /// <param name="dependent"></param>
    /// <returns></returns>
    public static async Task<ISet<Key>> ResolveInternalDependencies(this IContext context, Key dependent) {
      var resolveDependenciesKey = DependenciesKeys.ResolveInternalDependencies.In(dependent);
      return context.IsTaskDefined(resolveDependenciesKey) ? await context.Evaluate(resolveDependenciesKey) : ImmutableHashSet<Key>.Empty;
    }

    public static string GetNuGetRepositoryDir(this IConfig context) {
      return context.Evaluate(DependenciesKeys.NuGetRepositoryDir);
    }

    public static string GetFetchedPackagesFile(this IConfig context) {
      return Path.Combine(context.GetPersistentBuildConfigDir(), DependenciesSettings.FetchedPackagesFileName);
    }

    public static NuGetPackages GetNuGetResolvedPackages(this IConfig context) {
      return context.Evaluate(DependenciesKeys.NuGetResolvedPackages);
    }
  }
}