using System;
using System.Linq;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Plugins.Build;

namespace Bud.Plugins.Dependencies {
  public class DependenciesPlugin : IPlugin {
    public static readonly DependenciesPlugin Instance = new DependenciesPlugin();

    private DependenciesPlugin() {
    }

    public Settings ApplyTo(Settings settings, Key scope) {
      return settings
        .Apply(scope, BuildPlugin.Instance)
        .Init(DependenciesKeys.ScopeDependencies.In(scope), ImmutableList<ScopeDependency>.Empty)
        .Init(DependenciesKeys.ResolveScopeDependencies.In(scope), context => ResolveDependenciesImpl(context, scope))
        .Init(DependenciesKeys.ResolveScopeDependency.In(scope), context => ResolveScopeDependencyImpl(context, scope));
    }

    public static Task<ImmutableList<ResolvedScopeDependency>> ResolveDependenciesImpl(EvaluationContext context, Key scope) {
      var resolvedDependencies = Task.WhenAll(context.GetDependencies(scope).Select(dependency => dependency.Resolve(context)));
      return resolvedDependencies.ContinueWith<ImmutableList<ResolvedScopeDependency>>(completedTask => ImmutableList.CreateRange(completedTask.Result));
    }

    public static async Task<ResolvedScopeDependency> ResolveScopeDependencyImpl(EvaluationContext context, Key scope) {
      await context.BuildScope(scope);
      return new ResolvedScopeDependency(scope);
    }
  }
}

