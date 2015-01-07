using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bud.Plugins.Build;
using System.IO;

namespace Bud.Plugins.Dependencies {

  public static class Dependencies {
    public const string FetchedPackagesFileName = "nuGetPackages";

    public static IPlugin AddDependency(Key dependencyType, InternalDependency internalDependency, ExternalDependency fallbackExternalDependency, Predicate<IConfig> shouldUseInternalDependency) {
      return Plugin.Create((existingSettings, dependent) => 
        existingSettings
          .AddDependency(dependent, dependencyType, internalDependency, shouldUseInternalDependency)
          .AddDependency(dependent, dependencyType, fallbackExternalDependency, config => !shouldUseInternalDependency(config))
      );
    }

    public static IPlugin AddDependency(Key dependencyType, ExternalDependency dependency) {
      return Plugin.Create((existingSettings, dependent) => existingSettings.AddDependency(dependent, dependencyType, dependency));
    }

    public static IPlugin AddDependency(Key dependencyType, InternalDependency dependency) {
      return Plugin.Create((existingSettings, dependent) => existingSettings.AddDependency(dependent, dependencyType, dependency));
    }

    public static Settings AddDependency(this Settings settings, Key dependent, Key dependencyType, InternalDependency dependency, Predicate<IConfig> conditionForInclusion = null) {
      var dependenciesKey = GetInternalDependenciesKey(dependent, dependencyType);
      return settings
        .Init(dependenciesKey, ImmutableList<InternalDependency>.Empty)
        .Init(DependenciesKeys.ResolveInternalDependencies.In(dependenciesKey), context => ResolveInternalDependenciesImpl(context, dependent, dependencyType))
        .Modify(dependenciesKey, (config, dependencies) => conditionForInclusion == null || conditionForInclusion(config) ? dependencies.Add(dependency) : dependencies);
    }

    public static Settings AddDependency(this Settings settings, Key dependent, Key dependencyType, ExternalDependency dependency, Predicate<IConfig> conditionForInclusion = null) {
      var dependenciesKey = GetExternalDependenciesKey(dependent, dependencyType);
      return settings
        .Init(dependenciesKey, ImmutableList<ExternalDependency>.Empty)
        .Modify(DependenciesKeys.ExternalDependenciesKeys, (context, oldValue) => oldValue.Add(dependenciesKey))
        .Modify(dependenciesKey, (config, dependencies) => conditionForInclusion == null || conditionForInclusion(config) ? dependencies.Add(dependency) : dependencies);
    }

    public static ImmutableList<InternalDependency> GetInternalDependencies(this IConfig config, Key dependent, Key dependencyType) {
      var dependenciesKey = GetInternalDependenciesKey(dependent, dependencyType);
      return config.IsConfigDefined(dependenciesKey) ? config.Evaluate(dependenciesKey) : ImmutableList<InternalDependency>.Empty;
    }

    public static ImmutableList<ExternalDependency> GetExternalDependencies(this IConfig config, Key dependent, Key dependencyType) {
      var dependenciesKey = GetExternalDependenciesKey(dependent, dependencyType);
      return config.IsConfigDefined(dependenciesKey) ? config.Evaluate(dependenciesKey) : ImmutableList<ExternalDependency>.Empty;
    }

    public static ImmutableList<ExternalDependency> GetExternalDependencies(this IConfig context) {
      var keysWithNuGetDependencies = context.GetKeysWithExternalDependencies();
      return keysWithNuGetDependencies.SelectMany(key => context.Evaluate(key)).ToImmutableList();
    }

    public async static Task<ImmutableList<InternalDependency>> ResolveInternalDependencies(this IContext context, Key dependent, Key dependencyType) {
      var resolveDependenciesKey = DependenciesKeys.ResolveInternalDependencies.In(GetInternalDependenciesKey(dependent, dependencyType));
      return context.IsTaskDefined(resolveDependenciesKey) ? await context.Evaluate(resolveDependenciesKey) : ImmutableList<InternalDependency>.Empty;
    }

    private static Task<ImmutableList<InternalDependency>> ResolveInternalDependenciesImpl(IContext context, Key dependent, Key dependencyType) {
      return Task
        .WhenAll(context.GetInternalDependencies(dependent, dependencyType).Select(dependency => dependency.Resolve(context)))
        .ContinueWith<ImmutableList<InternalDependency>>(completedTask => completedTask.Result.ToImmutableList());
    }

    public static string GetNuGetRepositoryDir(this IConfig context) {
      return context.Evaluate(DependenciesKeys.NuGetRepositoryDir);
    }

    public static string GetFetchedPackagesFile(this IConfig context) {
      return Path.Combine(BuildDirs.GetPersistentBuildConfigDir(context), FetchedPackagesFileName);
    }

    public static NuGetResolution GetNuGetResolvedPackages(this IConfig context) {
      return context.Evaluate(DependenciesKeys.NuGetResolvedPackages);
    }

    private static ConfigKey<ImmutableList<InternalDependency>> GetInternalDependenciesKey(Key dependent, Key dependencyType) {
      return DependenciesKeys.InternalDependencies.In(dependencyType.In(dependent));
    }

    private static ConfigKey<ImmutableList<ExternalDependency>> GetExternalDependenciesKey(Key dependent, Key dependencyType) {
      return DependenciesKeys.ExternalDependencies.In(dependencyType.In(dependent));
    }

    private static ImmutableList<ConfigKey<ImmutableList<ExternalDependency>>> GetKeysWithExternalDependencies(this IConfig context) {
      return context.Evaluate(DependenciesKeys.ExternalDependenciesKeys);
    }
  }
}
