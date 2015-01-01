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

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .Apply(scope, BuildPlugin.Instance)
        .InitOrKeep(DependenciesKeys.ScopeDependencies.In(scope), ImmutableList<ScopeDependency>.Empty)
        .InitOrKeep(DependenciesKeys.ResolveScopeDependencies.In(scope), context => ResolveDependenciesImpl(context, scope))
        .InitOrKeep(DependenciesKeys.ResolveScopeDependency.In(scope), context => ResolveScopeDependencyImpl(context, scope));
    }

    public static Task<ImmutableList<ResolvedScopeDependency>> ResolveDependenciesImpl(EvaluationContext context, Scope scope) {
      var resolvedDependencies = Task.WhenAll(context.GetDependencies(scope).Select(dependency => dependency.Resolve(context)));
      return resolvedDependencies.ContinueWith<ImmutableList<ResolvedScopeDependency>>(completedTask => ImmutableList.CreateRange(completedTask.Result));
    }

    public static async Task<ResolvedScopeDependency> ResolveScopeDependencyImpl(EvaluationContext context, Scope scope) {
      await context.BuildScope(scope);
      return new ResolvedScopeDependency(scope);
    }
  }
}

