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
    /// Invokes the tasks on all internal dependencies transitively and returns their keys.
    /// </summary>
    public static async Task<ISet<Key>> EvaluateInternalDependencies(this IContext context, Key dependent) {
      var resolveDependenciesKey = DependenciesKeys.EvaluateInternalDependencies.In(dependent);
      return context.IsTaskDefined(resolveDependenciesKey) ? await context.Evaluate(resolveDependenciesKey) : ImmutableHashSet<Key>.Empty;
    }

    public static string GetFetchedDependenciesDir(this IConfig context) {
      return context.Evaluate(DependenciesKeys.FetchedDependenciesDir);
    }

    public static string GetFetchedDependenciesListFile(this IConfig context) {
      return context.Evaluate(DependenciesKeys.FetchedDependenciesListFile);
    }

    public static FetchedDependencies GetFetchedDependencies(this IConfig context) {
      return context.Evaluate(DependenciesKeys.FetchedDependencies);
    }

    public static IEnumerable<IDependency> GetDependencies(this IConfig config, Key key) {
      var dependenciesKey = DependenciesKeys.Dependencies.In(key);
      return config.IsConfigDefined(dependenciesKey) ? config.Evaluate(dependenciesKey) : ImmutableList<IDependency>.Empty;
    }
  }
}