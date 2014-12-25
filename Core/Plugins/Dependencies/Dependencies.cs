using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud.Plugins.Dependencies {
  public static class Dependencies {

    public static Settings Needs(this Settings dependent, Settings dependency) {
      return dependent.Needs(new ScopeDependency(dependency.CurrentScope));
    }

    public static Settings Needs(this Settings dependent, ScopeDependency dependency) {
      return dependent.Modify(DependenciesKeys.ScopeDependencies.In(dependent.CurrentScope), dependencies => dependencies.Add(dependency));
    }

    public static ImmutableList<ScopeDependency> GetDependencies(this EvaluationContext context, Scope inScope) {
      return context.Evaluate(DependenciesKeys.ScopeDependencies.In(inScope));
    }

    public static Task<ImmutableList<ResolvedScopeDependency>> ResolveDependencies(this EvaluationContext context, Scope inScope) {
      return context.Evaluate(DependenciesKeys.ResolveScopeDependencies.In(inScope));
    }
  }
}
