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

    public static Settings Needs(this Settings dependent, IDependency dependency) {
      return dependent.Modify(DependenciesKeys.Dependencies.In(dependent.CurrentScope), dependencies => dependencies.Add(dependency));
    }

    public static ImmutableList<IDependency> GetDependencies(this EvaluationContext context, Scope inScope) {
      return context.Evaluate(DependenciesKeys.Dependencies.In(inScope));
    }

    public static IEnumerable<ScopeDependency> GetScopeDependencies(this EvaluationContext context, Scope inScope) {
      return context.GetDependencies(inScope).Where(dependency => dependency is ScopeDependency).OfType<ScopeDependency>();
    }

    public static Task<ImmutableList<IResolvedDependency>> ResolveDependencies(this EvaluationContext context, Scope inScope) {
      return context.Evaluate(DependenciesKeys.ResolveDependencies.In(inScope));
    }
  }
}
