using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

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
                             DependenciesKeys.ResolveInternalDependencies.Init(ResolveInternalDependenciesImpl));
    }

    public static Setup AddDependency(ExternalDependency dependency, Predicate<IConfig> conditionForInclusion = null) {
      return settings => settings.Do(DependenciesKeys.ExternalDependencies.Init(ImmutableList<ExternalDependency>.Empty),
                                     DependenciesKeys.ExternalDependencies.Modify((config, dependencies) => conditionForInclusion == null || conditionForInclusion(config) ? dependencies.Add(dependency) : dependencies))
                                 .Globally(DependenciesKeys.ExternalDependenciesKeys.Modify((ctxt, oldValue) => oldValue.Add(DependenciesKeys.ExternalDependencies.In(settings.Scope))));
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
      var transitiveDependencies = await ResolveInternalDependenciesImpl(context, dependency.DepdendencyTarget);
      return new[] {dependency.DepdendencyTarget}.Concat(transitiveDependencies);
    }
  }
}