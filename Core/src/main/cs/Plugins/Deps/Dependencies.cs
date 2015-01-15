using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Plugins.Build;

namespace Bud.Plugins.Deps {
  public static class Dependencies {
    public const string FetchedPackagesFileName = "nuGetPackages";

    public static IPlugin AddDependency(InternalDependency internalDependency,
                                        ExternalDependency fallbackExternalDependency,
                                        Predicate<IConfig> shouldUseInternalDependency) {
      return PluginUtils.Create((existingSettings, dependent) =>
        existingSettings
          .AddDependency(dependent, internalDependency, shouldUseInternalDependency)
          .AddDependency(dependent, fallbackExternalDependency, config => !shouldUseInternalDependency(config))
        );
    }

    public static IPlugin AddDependency(ExternalDependency dependency) {
      return PluginUtils.Create((existingSettings, dependent) => existingSettings.AddDependency(dependent, dependency));
    }

    public static IPlugin AddDependency(InternalDependency dependency) {
      return PluginUtils.Create((existingSettings, dependent) => existingSettings.AddDependency(dependent, dependency));
    }

    public static Settings AddDependency(this Settings settings, Key dependent, InternalDependency dependency, Predicate<IConfig> conditionForInclusion = null) {
      var dependenciesKey = GetInternalDependenciesKey(dependent);
      return settings
        .Init(dependenciesKey, ImmutableList<InternalDependency>.Empty)
        .Init(DependenciesKeys.ResolveInternalDependencies.In(dependenciesKey), context => ResolveInternalDependenciesImpl(context, dependent))
        .Modify(dependenciesKey, (config, dependencies) => conditionForInclusion == null || conditionForInclusion(config) ? dependencies.Add(dependency) : dependencies);
    }

    public static Settings AddDependency(this Settings settings, Key dependent, ExternalDependency dependency, Predicate<IConfig> conditionForInclusion = null) {
      var dependenciesKey = GetExternalDependenciesKey(dependent);
      return settings
        .Init(dependenciesKey, ImmutableList<ExternalDependency>.Empty)
        .Modify(DependenciesKeys.ExternalDependenciesKeys, (context, oldValue) => oldValue.Add(dependenciesKey))
        .Modify(dependenciesKey, (config, dependencies) => conditionForInclusion == null || conditionForInclusion(config) ? dependencies.Add(dependency) : dependencies);
    }

    public static ImmutableList<InternalDependency> GetInternalDependencies(this IConfig config, Key dependent) {
      var dependenciesKey = GetInternalDependenciesKey(dependent);
      return config.IsConfigDefined(dependenciesKey) ? config.Evaluate(dependenciesKey) : ImmutableList<InternalDependency>.Empty;
    }

    public static ImmutableList<ExternalDependency> GetExternalDependencies(this IConfig config, Key dependent) {
      var dependenciesKey = GetExternalDependenciesKey(dependent);
      return config.IsConfigDefined(dependenciesKey) ? config.Evaluate(dependenciesKey) : ImmutableList<ExternalDependency>.Empty;
    }

    public static ImmutableList<ExternalDependency> GetExternalDependencies(this IConfig context) {
      var keysWithNuGetDependencies = context.GetKeysWithExternalDependencies();
      return keysWithNuGetDependencies.SelectMany(key => context.Evaluate(key)).ToImmutableList();
    }

    public static async Task<ISet<Key>> ResolveInternalDependencies(this IContext context, Key dependent) {
      var internalDependenciesKey = GetInternalDependenciesKey(dependent);
      var resolveDependenciesKey = DependenciesKeys.ResolveInternalDependencies.In(internalDependenciesKey);
      return context.IsTaskDefined(resolveDependenciesKey) ? await context.Evaluate(resolveDependenciesKey) : ImmutableHashSet<Key>.Empty;
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
      return Enumerable.Concat(new[] {dependency.DepdendencyTarget}, transitiveDependencies);
    }

    public static string GetNuGetRepositoryDir(this IConfig context) {
      return context.Evaluate(DependenciesKeys.NuGetRepositoryDir);
    }

    public static string GetFetchedPackagesFile(this IConfig context) {
      return Path.Combine(BuildDirs.GetPersistentBuildConfigDir(context), FetchedPackagesFileName);
    }

    public static NuGetPackages GetNuGetResolvedPackages(this IConfig context) {
      return context.Evaluate(DependenciesKeys.NuGetResolvedPackages);
    }

    private static ConfigKey<ImmutableList<InternalDependency>> GetInternalDependenciesKey(Key dependent) {
      return DependenciesKeys.InternalDependencies.In(dependent);
    }

    private static ConfigKey<ImmutableList<ExternalDependency>> GetExternalDependenciesKey(Key dependent) {
      return DependenciesKeys.ExternalDependencies.In(dependent);
    }

    private static ImmutableList<ConfigKey<ImmutableList<ExternalDependency>>> GetKeysWithExternalDependencies(this IConfig context) {
      return context.Evaluate(DependenciesKeys.ExternalDependenciesKeys);
    }
  }
}