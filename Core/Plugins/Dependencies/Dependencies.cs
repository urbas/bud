using System;

namespace Bud.Plugins.Dependencies {
  public static class Dependencies {

    public static Settings DependsOn(this Settings dependent, Settings dependency) {
      return dependent.DependsOn(new Dependency(dependent.CurrentScope));
    }

    public static Settings DependsOn(this Settings dependent, Dependency dependency) {
      return dependent.Modify(DependenciesKeys.Dependencies, dependencies => dependencies.Add(dependency.Scope, dependency));
    }

    public static Settings ProvidesDependency(this Settings dependent, DependencySource dependencySource) {
      return dependent.Modify(DependenciesKeys.DependencySources, dependencySources => dependencySources.Add(dependencySource.ScopeOfProvider, dependencySource));
    }
  }
}

