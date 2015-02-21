using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NuGet;

namespace Bud.Dependencies {
  public static class DependenciesSettings {
    public const string FetchedPackagesFileName = "nuGetPackages.json";

    public static Setup AddDependency(InternalDependency internalDependency,
                                      ExternalDependency fallbackExternalDependency,
                                      Predicate<IConfig> shouldUseInternalDependency) {
      return Settings.Modify(AddDependency(internalDependency, shouldUseInternalDependency),
                             AddDependency(fallbackExternalDependency, config => !shouldUseInternalDependency(config)));
    }

    public static Setup AddDependency(InternalDependency dependency, Predicate<IConfig> conditionForInclusion = null) {
      return Settings.Modify(DependenciesKeys.InternalDependencies.Init(ImmutableList<InternalDependency>.Empty),
                             DependenciesKeys.InternalDependencies.Modify((config, dependencies) => conditionForInclusion == null || conditionForInclusion(config) ? dependencies.Add(dependency) : dependencies),
                             DependenciesKeys.EvaluateInternalDependencies.Init(ResolveInternalDependenciesImpl),
                             DependenciesKeys.Dependencies.Init(DependenciesImpl));
    }

    public static Setup AddDependency(ExternalDependency dependency, Predicate<IConfig> conditionForInclusion = null) {
      return settings => settings.Do(DependenciesKeys.ExternalDependencies.Init(ImmutableList<ExternalDependency>.Empty),
                                     DependenciesKeys.ExternalDependencies.Modify((config, dependencies) => conditionForInclusion == null || conditionForInclusion(config) ? dependencies.Add(dependency) : dependencies),
                                     DependenciesKeys.Dependencies.Init(DependenciesImpl))
                                 .Globally(DependenciesKeys.ExternalDependenciesKeys.Modify((ctxt, oldValue) => oldValue.Add(settings.Scope / DependenciesKeys.ExternalDependencies)));
    }

    private static Task<ISet<Key>> ResolveInternalDependenciesImpl(IContext context, Key dependent) {
      return Task
        .WhenAll(context.GetInternalDependencies(dependent).Select(dependency => ResolveDependencyImpl(context, dependency)))
        .ContinueWith<ISet<Key>>(completedTask => completedTask.Result.Aggregate(ImmutableHashSet.CreateBuilder<Key>(), (builder, dependencies) => {
          builder.UnionWith(dependencies);
          return builder;
        }).ToImmutable());
    }

    private static async Task<IEnumerable<Key>> ResolveDependencyImpl(IContext context, InternalDependency dependency) {
      await dependency.Resolve(context);
      var transitiveDependencies = await ResolveInternalDependenciesImpl(context, dependency.DependencyTarget);
      return new[] {dependency.DependencyTarget}.Concat(transitiveDependencies);
    }

    private static IEnumerable<IDependency> DependenciesImpl(IConfig config, Key project) {
      // TODO: Get only dependencies that are meant for the current target framework.
      var internalDependencies = config.GetInternalDependencies(project);
      var transitiveDependencies = internalDependencies.SelectMany(dependency => config.GetDependencies(dependency.DependencyTarget));
      var directExternalDependencies = config.GetExternalDependencies(project);
      var fetchedDependencies = config.GetFetchedDependencies();
      IEnumerable<Package> directExternalPackages = directExternalDependencies.Select(fetchedDependencies.GetPackage);
      return internalDependencies.Concat(transitiveDependencies)
                                 .Concat(GetExternalDependenciesTransitive(directExternalPackages, fetchedDependencies))
                                 .Distinct(new DependencyEquality(config));
    }

    private static IEnumerable<Package> GetExternalDependenciesTransitive(IEnumerable<Package> directDependencies, FetchedDependencies fetchedDependencies) {
      // TODO: Get only dependencies that are meant for the current target framework.
      return directDependencies.Concat(directDependencies.SelectMany(directDependency => GetExternalDependenciesTransitive(directDependency.Dependencies, fetchedDependencies)));
    }

    private static IEnumerable<Package> GetExternalDependenciesTransitive(IEnumerable<PackageDependencyInfo> directDependencies, FetchedDependencies fetchedDependencies) {
      return GetExternalDependenciesTransitive(directDependencies.Select(directDependency => fetchedDependencies.GetPackage(directDependency.Id, directDependency.VersionSpec)), fetchedDependencies);
    }
  }
}