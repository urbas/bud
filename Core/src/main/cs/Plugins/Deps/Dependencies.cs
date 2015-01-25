﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Plugins.Build;

namespace Bud.Plugins.Deps {
  public static class Dependencies {
    public const string FetchedPackagesFileName = "nuGetPackages.json";

    public static Func<Settings, Settings> AddDependency(InternalDependency internalDependency,
                                        ExternalDependency fallbackExternalDependency,
                                        Predicate<IConfig> shouldUseInternalDependency) {
      return existingSettings => existingSettings
          .AddDependency(internalDependency, shouldUseInternalDependency)
          .AddDependency(fallbackExternalDependency, config => !shouldUseInternalDependency(config));
    }

    public static IPlugin AddDependency(ExternalDependency dependency) {
      return PluginUtils.Create(existingSettings => existingSettings.AddDependency(dependency));
    }

    public static IPlugin AddDependency(InternalDependency dependency) {
      return PluginUtils.Create(existingSettings => existingSettings.AddDependency(dependency));
    }

    public static Settings AddDependency(this Settings settings, InternalDependency dependency, Predicate<IConfig> conditionForInclusion = null) {
      return settings.Do(
          DependenciesKeys.InternalDependencies.Init(ImmutableList<InternalDependency>.Empty),
          DependenciesKeys.InternalDependencies.Modify((config, dependencies) => conditionForInclusion == null || conditionForInclusion(config) ? dependencies.Add(dependency) : dependencies),
          DependenciesKeys.ResolveInternalDependencies.In(DependenciesKeys.InternalDependencies).Init(context => ResolveInternalDependenciesImpl(context, settings.Scope))
        );
    }

    public static Settings AddDependency(this Settings settings, ExternalDependency dependency, Predicate<IConfig> conditionForInclusion = null) {
      return settings.Do(
          DependenciesKeys.ExternalDependencies.Init(ImmutableList<ExternalDependency>.Empty),
          DependenciesKeys.ExternalDependencies.Modify((config, dependencies) => conditionForInclusion == null || conditionForInclusion(config) ? dependencies.Add(dependency) : dependencies),
          DependenciesKeys.ResolveInternalDependencies.In(DependenciesKeys.ExternalDependencies).Init(context => ResolveInternalDependenciesImpl(context, settings.Scope))
        ).In(Key.Global,
          DependenciesKeys.ExternalDependenciesKeys.Modify(oldValue => oldValue.Add(DependenciesKeys.ExternalDependencies.In(settings.Scope)))
        );
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

    private static ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>> GetKeysWithExternalDependencies(this IConfig context) {
      return context.Evaluate(DependenciesKeys.ExternalDependenciesKeys);
    }
  }
}