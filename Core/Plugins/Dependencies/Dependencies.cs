using System;
using System.Collections.Immutable;

namespace Bud.Plugins.Dependencies {
  public static class Dependencies {

    public static Settings DependsOn(this Settings dependent, Settings dependency) {
      return dependent.DependsOn(new Dependency(dependency.CurrentScope));
    }

    public static Settings DependsOn(this Settings dependent, Dependency dependency) {
      return dependent.Modify(DependenciesKeys.Dependencies.In(dependent.CurrentScope), dependencies => dependencies.Add(dependency));
    }

    public static ImmutableList<Dependency> GetDependencies(this EvaluationContext context, Scope inScope) {
      return context.Evaluate(DependenciesKeys.Dependencies.In(inScope));
    }
  }
}

