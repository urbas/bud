using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud.Plugins.Dependencies {

  public static class Dependencies {

    public static IPlugin Add(Key dependencyType, TaskKey dependency) {
      return Plugin.Create((existingSettings, dependent) => existingSettings.AddDependency(dependent, dependencyType, dependency));
    }

    public static Settings AddDependency(this Settings settings, Key dependent, Key dependencyType, TaskKey dependency) {
      var dependenciesKey = GetDependenciesKey(dependent, dependencyType);
      return settings
        .Init(dependenciesKey, ImmutableList<TaskKey>.Empty)
        .Init(DependenciesKeys.ResolveDependencies.In(dependenciesKey), context => ResolveDependenciesImpl(context, dependent, dependencyType))
        .Modify(dependenciesKey, dependencies => dependencies.Add(dependency));
    }

    public static ImmutableList<TaskKey> GetDependencies(this IContext context, Key dependent, Key dependencyType) {
      var dependenciesKey = GetDependenciesKey(dependent, dependencyType);
      return context.Evaluate(dependenciesKey);
    }

    public async static Task<ImmutableList<TaskKey>> ResolveDependencies(this IContext context, Key dependent, Key dependencyType) {
      var dependenciesKey = DependenciesKeys.ResolveDependencies.In(GetDependenciesKey(dependent, dependencyType));
      return context.IsTaskDefined(dependenciesKey) ? await context.Evaluate(dependenciesKey) : ImmutableList<TaskKey>.Empty;
    }

    private static ConfigKey<ImmutableList<TaskKey>> GetDependenciesKey(Key dependent, Key dependencyType) {
      return DependenciesKeys.Dependencies.In(dependencyType.In(dependent));
    }

    private static Task<ImmutableList<TaskKey>> ResolveDependenciesImpl(IContext context, Key dependent, Key dependencyType) {
      return Task
        .WhenAll(context.GetDependencies(dependent, dependencyType).Select(dependency => ResolveDependency(context, dependency)))
        .ContinueWith<ImmutableList<TaskKey>>(completedTask => ImmutableList.CreateRange(completedTask.Result));
    }

    private async static Task<TaskKey> ResolveDependency(IContext context, TaskKey dependency) {
      await context.EvaluateTask(dependency);
      return dependency;
    }
  }
}
