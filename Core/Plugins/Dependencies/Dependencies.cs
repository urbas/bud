using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud.Plugins.Dependencies {
  public static class Dependencies {

    public static Plugin ToDependency(this Scope scope) {
      return Plugin.Create((existingSettings, dependentScope) => existingSettings.AddDependency(dependentScope, new ScopeDependency(scope)));
    }

    public static Settings AddDependency(this Settings settings, Scope dependent, ScopeDependency dependency) {
      return settings.Modify(DependenciesKeys.ScopeDependencies.In(dependent), dependencies => dependencies.Add(dependency));
    }

    public static ImmutableList<ScopeDependency> GetDependencies(this EvaluationContext context, Scope inScope) {
      return context.Evaluate(DependenciesKeys.ScopeDependencies.In(inScope));
    }

    public static Task<ImmutableList<ResolvedScopeDependency>> ResolveDependencies(this EvaluationContext context, Scope inScope) {
      return context.Evaluate(DependenciesKeys.ResolveScopeDependencies.In(inScope));
    }
  }
}
